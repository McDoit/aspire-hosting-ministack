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
    public void StackportContainerImageTags_UseExpectedValues()
    {
        Assert.Equal("docker.io", StackportContainerImageTags.Registry);
        Assert.Equal("davireis/stackport", StackportContainerImageTags.Image);
        Assert.Equal("latest", StackportContainerImageTags.Tag);
    }

    [Fact]
    public void WithStackport_AddsStackportContainerResource()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        builder.AddMinistack(awsConfig).WithStackport();

        var stackportResource = builder.Resources.OfType<ContainerResource>()
            .SingleOrDefault(r => r.Name == "stackport");

        Assert.NotNull(stackportResource);
    }

    [Fact]
    public void WithStackport_StackportIsExcludedFromManifest()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        builder.AddMinistack(awsConfig).WithStackport();

        var stackportResource = builder.Resources.OfType<ContainerResource>()
            .Single(r => r.Name == "stackport");

        Assert.True(stackportResource.Annotations.OfType<ManifestPublishingCallbackAnnotation>().Any());
    }

    [Fact]
    public void WithStackport_UsesCorrectContainerImage()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        builder.AddMinistack(awsConfig).WithStackport();

        var stackportResource = builder.Resources.OfType<ContainerResource>()
            .Single(r => r.Name == "stackport");

        var imageAnnotation = Assert.Single(stackportResource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(StackportContainerImageTags.Registry, imageAnnotation.Registry);
        Assert.Equal(StackportContainerImageTags.Image, imageAnnotation.Image);
        Assert.Equal(StackportContainerImageTags.Tag, imageAnnotation.Tag);
    }

    [Fact]
    public void WithStackport_ExposesHttpEndpointOnPort8080()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        builder.AddMinistack(awsConfig).WithStackport();

        var stackportResource = builder.Resources.OfType<ContainerResource>()
            .Single(r => r.Name == "stackport");

        var endpoint = Assert.Single(stackportResource.Annotations.OfType<EndpointAnnotation>(), e => e.Name == "http");
        Assert.Equal(8080, endpoint.TargetPort);
    }

    [Fact]
    public void WithStackport_ReturnsMinistackBuilder()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);
        var ministackBuilder = builder.AddMinistack(awsConfig);

        var returnedBuilder = ministackBuilder.WithStackport();

        Assert.Same(ministackBuilder, returnedBuilder);
    }

    [Fact]
    public void WithStackport_AcceptsCustomName()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.USEast1);

        builder.AddMinistack(awsConfig).WithStackport(name: "my-stackport");

        var stackportResource = builder.Resources.OfType<ContainerResource>()
            .SingleOrDefault(r => r.Name == "my-stackport");

        Assert.NotNull(stackportResource);
    }

    [Fact]
    public void AddMinistack_StoresRegionOnResource()
    {
        var builder = DistributedApplication.CreateBuilder();
        var awsConfig = builder.AddAWSSDKConfig().WithRegion(RegionEndpoint.EUWest1);

        var ministackBuilder = builder.AddMinistack(awsConfig);

        Assert.Equal(RegionEndpoint.EUWest1.SystemName, ministackBuilder.Resource.Region);
    }
}

