using Amazon;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using McDoit.Aspire.Hosting.Ministack.Helpers;
using McDoit.Aspire.Hosting.Ministack.Resources;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

public class MinistackHostingExtensionsTests
{
    [Fact]
    public void ConnectionStringExpression_IsCreated()
    {
        var resource = new MinistackResource("ministack");

        var expression = resource.ConnectionStringExpression;

        Assert.NotNull(expression);
    }

    [Fact]
    public void HttpEndpointName_UsesExpectedValue()
    {
        Assert.Equal("http", MinistackResource.HttpEndpointName);
    }

    [Fact]
    public void ContainerImageTags_UseExpectedValues()
    {
        Assert.Equal("docker.io", MinistackContainerImageTags.Registry);
        Assert.Equal("nahuelnucera/ministack", MinistackContainerImageTags.Image);
        Assert.Equal("latest", MinistackContainerImageTags.Tag);
    }

    [Fact]
    public void AddMinistack_IsExcludedFromManifest()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig);

        Assert.True(ministackBuilder.Resource.Annotations.OfType<ManifestPublishingCallbackAnnotation>().Any());
    }

    [Fact]
    public async Task AddMinistack_WithRedisHost_SetsRedisHostEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig, configureContainer: o => o.RedisHost = "my-redis");

        var env = await GetEnvironmentVariablesAsync(ministackBuilder.Resource);
        Assert.Equal("my-redis", env["REDIS_HOST"]);
    }

    [Fact]
    public async Task AddMinistack_WithoutRedisHost_DoesNotSetRedisHostEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig);

        var env = await GetEnvironmentVariablesAsync(ministackBuilder.Resource);
        Assert.False(env.ContainsKey("REDIS_HOST"));
    }

    [Fact]
    public async Task AddMinistack_WithAccountId_SetsAccountIdEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig, configureContainer: o => o.AccountId = "123456789012");

        var env = await GetEnvironmentVariablesAsync(ministackBuilder.Resource);
        Assert.Equal("123456789012", env["MINISTACK_ACCOUNT_ID"]);
    }

    [Fact]
    public async Task AddMinistack_WithoutAccountId_DoesNotSetAccountIdEnvironmentVariable()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig);

        var env = await GetEnvironmentVariablesAsync(ministackBuilder.Resource);
        Assert.False(env.ContainsKey("MINISTACK_ACCOUNT_ID"));
    }

    private static async Task<Dictionary<string, object>> GetEnvironmentVariablesAsync(IResource resource)
    {
        var envVars = new Dictionary<string, object>();
        var execCtx = new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run);
        var context = new EnvironmentCallbackContext(execCtx, envVars, CancellationToken.None);
        foreach (var annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
        {
            await annotation.Callback(context);
        }
        return envVars;
    }
}
