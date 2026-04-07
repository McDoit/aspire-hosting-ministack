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
using Microsoft.VisualStudio.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace McDoit.Aspire.Hosting.Ministack;

public static class MinistackResourceBuilderExtensions
{
	/// <summary>
	/// Adds the <see href="https://hub.docker.com/r/davireis/stackport">Stackport</see> web UI
	/// container to the given <paramref name="builder"/> instance, connected to the Ministack resource.
	/// Stackport is a universal AWS resource browser for local emulators.
	/// </summary>
	/// <param name="builder">The <see cref="IResourceBuilder{MinistackResource}"/>.</param>
	/// <param name="name">The name of the Stackport resource. Defaults to <c>"stackport"</c>.</param>
	/// <param name="port">An optional host port to expose the Stackport web UI on.</param>
	/// <returns>
	/// The original <see cref="IResourceBuilder{MinistackResource}"/> instance to allow chaining.
	/// </returns>
	public static IResourceBuilder<MinistackResource> WithStackport(
		this IResourceBuilder<MinistackResource> builder,
		[ResourceName] string name = "stackport",
		int? port = null)
	{
		builder.ApplicationBuilder
			.AddResource(new ContainerResource(name))
			.WithImage(StackportContainerImageTags.Image)
			.WithImageRegistry(StackportContainerImageTags.Registry)
			.WithImageTag(StackportContainerImageTags.Tag)
			.WithEnvironment(ctx =>
			{
				ctx.EnvironmentVariables["AWS_ENDPOINT_URL"] = builder.Resource.ConnectionStringExpression;
				ctx.EnvironmentVariables["AWS_REGION"] = builder.Resource.Region.SystemName;
				ctx.EnvironmentVariables["AWS_ACCESS_KEY_ID"] = "ministack";
				ctx.EnvironmentVariables["AWS_SECRET_ACCESS_KEY"] = "ministack";
			})
			.WithHttpEndpoint(port: port, targetPort: 8080, name: "http", isProxied: !port.HasValue)
			.WithHttpHealthCheck(path: "/api/health")
			.WaitFor(builder)
			.ExcludeFromManifest();

		return builder;
	}

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
		var prefix = $"{builder.Environment.ApplicationName}-{name}";

		var profileName = $"{prefix}-aspire-profile";

		var resource = new MinistackResource(name, aWSSDKConfig.Region ?? RegionEndpoint.USEast1, profileName);

		var options = new MinistackContainerOptions();
		
		configureContainer?.Invoke(options);

        var ministackBuilder = builder.AddResource(resource)
					  .WithImage(options.Image ?? MinistackContainerImageTags.Image)
					  .WithImageRegistry(options.Registry ?? MinistackContainerImageTags.Registry)
					  .WithImageTag(options.Tag ?? MinistackContainerImageTags.Tag)
					  .WithEnvironment("MINISTACK_REGION", resource.Region.SystemName)
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

	/// <summary>
	/// Runs <c>npx cdk bootstrap</c> against the Ministack instance when it becomes available.
	/// </summary>
	/// <param name="builder">The <see cref="IResourceBuilder{MinistackResource}"/>.</param>
	/// <param name="qualifier">An optional CDK bootstrap qualifier passed via <c>--qualifier</c>. Must contain only alphanumeric characters and hyphens.</param>
	/// <returns>The same <see cref="IResourceBuilder{MinistackResource}"/> for chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="qualifier"/> contains characters other than alphanumeric characters and hyphens.</exception>
	public static IResourceBuilder<MinistackResource> WithCdkBootstrap(
		this IResourceBuilder<MinistackResource> builder,
		string? qualifier = null)
	{
		if (!string.IsNullOrEmpty(qualifier) && !System.Text.RegularExpressions.Regex.IsMatch(qualifier, @"^[a-zA-Z0-9\-]+$"))
		{
			throw new ArgumentException("The CDK bootstrap qualifier must contain only alphanumeric characters and hyphens.", nameof(qualifier));
		}
		
		builder.WithAnnotation(new CdkBootstrapAnnotation(qualifier));


		builder.OnResourceReady(async (resource, _, cancellationToken) =>
		{
			try
			{
              static string QuoteForCmd(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
				static string QuoteForSh(string value) => $"'{value.Replace("'", "'\"'\"'")}'";

				var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);

				if (string.IsNullOrEmpty(connectionString))
				{
					throw new InvalidOperationException("Ministack connection string is not available.");
				}

				// Ministack uses a fake AWS account ID of 000000000000.
				const string fakeAccountId = "000000000000";
				var region = resource.Region.SystemName;

				// CDK requires an explicit environment (aws://ACCOUNT/REGION) when a custom
				// endpoint URL is set, otherwise it exits with "Specify an environment name".
                var profileArgument = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? QuoteForCmd(resource.ProfileName)
					: QuoteForSh(resource.ProfileName);

				var npxCommand = $"npx --yes cdk bootstrap aws://{fakeAccountId}/{region} --profile {profileArgument}";
				
				if (!string.IsNullOrEmpty(qualifier))
				{
                   var qualifierArgument = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
						? QuoteForCmd(qualifier)
						: QuoteForSh(qualifier);

					npxCommand += $" --qualifier {qualifierArgument}";
				}

				var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
             var shellFileName = isWindows ? "cmd.exe" : "/bin/sh";
				var shellArguments = isWindows
					? $"/d /s /c \"{npxCommand}\""
                    : $"-lc {QuoteForSh(npxCommand)}";

				using var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
                        FileName = shellFileName,
						Arguments = shellArguments,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						RedirectStandardInput = false,
						CreateNoWindow = true,
						WorkingDirectory = Environment.CurrentDirectory,

					},
					EnableRaisingEvents = true,
				};				

				process.StartInfo.EnvironmentVariables["AWS_ENDPOINT_URL"] = connectionString;
				process.StartInfo.EnvironmentVariables["AWS_ACCESS_KEY_ID"] = "ministack";
				process.StartInfo.EnvironmentVariables["AWS_SECRET_ACCESS_KEY"] = "ministack";
				process.StartInfo.EnvironmentVariables["AWS_DEFAULT_REGION"] = region;

				process.OutputDataReceived += (c, e) =>
				{
					if (!string.IsNullOrWhiteSpace(e.Data))
						Console.WriteLine($"[cdk:out] {e.Data}");
				};

				process.ErrorDataReceived += (c, e) =>
				{
					if (!string.IsNullOrWhiteSpace(e.Data))
						Console.Error.WriteLine($"[cdk:err] {e.Data}");
				};

				if (!process.Start())
					throw new InvalidOperationException("Failed to start process.");

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
				using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

				try
				{
					await process.WaitForExitAsync(linkedCts.Token);
				}
				catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
				{
					if (!process.HasExited)
						process.Kill(entireProcessTree: true);

					throw new TimeoutException("CDK bootstrap timed out after 1 minute.");
				}

              if (process.ExitCode != 0)
					throw new InvalidOperationException($"'{npxCommand}' exited with code {process.ExitCode}.");
			}
			catch (Exception)
			{
				//TODO add bootstrap resource logging
				throw;
			}
		});

		return builder;
	}
}

