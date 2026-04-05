// Put extensions in the Aspire.Hosting namespace to ease discovery as referencing
// the Aspire hosting package automatically adds this namespace.
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.AWS;
using McDoit.Aspire.Hosting.Ministack.Helpers;
using McDoit.Aspire.Hosting.Ministack.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace McDoit.Aspire.Hosting.Ministack;

public static class MinistackResourceBuilderExtensions
{
	/// <summary>
	/// Adds the <see cref="MinistackResource"/> to the given
	/// <paramref name="builder"/> instance.
	/// </summary>
	/// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
	/// <param name="aWSSDKConfig">The <see cref="IAWSSDKConfig"/>.</param>
	/// <param name="name">The name of the resource.</param>
	/// <param name="configureContainer">An action to configure the <see cref="MinistackContainerOptions"/>.</param>
	/// <returns>
	/// An <see cref="IResourceBuilder{MinistackResource}"/> instance that
	/// represents the added Ministack resource.
	/// </returns>
	public static IResourceBuilder<MinistackResource> AddMinistack(
		this IDistributedApplicationBuilder builder,
		IAWSSDKConfig aWSSDKConfig,
		[ResourceName] string name = "ministack",
		Action<MinistackContainerOptions>? configureContainer = null)
	{
		var resource = new MinistackResource(name);

		var options = new MinistackContainerOptions();
		
		configureContainer?.Invoke(options);

        var ministackBuilder = builder.AddResource(resource)
					  .WithImage(options.Image ?? MinistackContainerImageTags.Image)
					  .WithImageRegistry(options.Registry ?? MinistackContainerImageTags.Registry)
					  .WithImageTag(options.Tag ?? MinistackContainerImageTags.Tag)
					  .WithEnvironment("MINISTACK_REGION", aWSSDKConfig.Region?.SystemName ?? RegionEndpoint.USEast1.SystemName)
					  .WithHttpEndpoint(
						  port: options.Port,
						  targetPort: 4566,
						  name: MinistackResource.HttpEndpointName,
						  isProxied: !options.Port.HasValue)
					  .WithHttpHealthCheck(path: "/_ministack/health")
					  .ExcludeFromManifest();

		if(options.Lifetime == ContainerLifetime.Persistent)
		{
			ministackBuilder.WithEnvironment("PERSIST_STATE", "1");
			ministackBuilder.WithEnvironment("S3_PERSIST", "1");
		}

		var prefix = $"{builder.Environment.ApplicationName}-{name}";

		var profileInitDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		builder.Services.AddHealthChecks().AddCheck($"{prefix}-profile-init", () =>
		{
			return profileInitDone.Task.Status switch
			{
				TaskStatus.RanToCompletion => HealthCheckResult.Healthy(),
				TaskStatus.Faulted or TaskStatus.Canceled => HealthCheckResult.Unhealthy("Failure during profile initialization."),				
				_ => HealthCheckResult.Degraded("Profile initialization is still in progress."),
			};
		});

		ministackBuilder.WithHealthCheck($"{prefix}-profile-init");

		var profileName = $"{prefix}-aspire-profile";

		aWSSDKConfig.WithProfile(profileName);

		ministackBuilder.OnConnectionStringAvailable(async (ministackResource, connectionStringAvailableEvent, cancellationToken) =>
		{
			try
			{
				var connectionString = await ministackResource.ConnectionStringExpression.GetValueAsync(cancellationToken);

				var scf = new SharedCredentialsFile();

				if (scf.TryGetProfile(profileName, out var profile))
				{
					scf.UnregisterProfile(profileName); // Ensure no existing profile with the same name
				}

				var localStackProfile = new CredentialProfile(profileName, new CredentialProfileOptions
				{
					AccessKey = "ministack",
					SecretKey = "ministack",
				})
				{
					EndpointUrl = connectionString,
				};

				scf.RegisterProfile(localStackProfile);

				profileInitDone.SetResult();
			}
			catch (Exception exc)
			{
				profileInitDone.TrySetException(exc);
				throw;
			}
		});		

		return ministackBuilder;
	}
}
