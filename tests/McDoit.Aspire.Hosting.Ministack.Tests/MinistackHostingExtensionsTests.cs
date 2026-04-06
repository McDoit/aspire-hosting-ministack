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
    public void WithCdkBootstrap_ReturnsBuilder()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig);
        var result = ministackBuilder.WithCdkBootstrap();

        Assert.Same(ministackBuilder, result);
    }

    [Fact]
    public void WithCdkBootstrap_WithQualifier_ReturnsBuilder()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        var ministackBuilder = builder.AddMinistack(awsConfig);
        var result = ministackBuilder.WithCdkBootstrap("myqualifier");

        Assert.Same(ministackBuilder, result);
    }

    [Fact]
    public void WithCdkBootstrap_CanBeChained()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        // Should not throw; fluent chaining should work
        var result = builder.AddMinistack(awsConfig).WithCdkBootstrap();

        Assert.NotNull(result);
    }
}
