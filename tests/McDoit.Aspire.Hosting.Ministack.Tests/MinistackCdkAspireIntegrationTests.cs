using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using McDoit.Aspire.Hosting.Ministack.Resources;
using McDoit.Aspire.Hosting.Ministack.Helpers;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

public class MinistackCdkAspireIntegrationTests(CdkAspireTestingFixture fixture) : IClassFixture<CdkAspireTestingFixture>
{
    [Fact]
    public async Task CdkSampleAppHost_CreatesMinistackResource()
    {
        var builder = await fixture.CreateBuilderAsync();

        await using var app = await builder.BuildAsync();

        var resource = Assert.Single(builder.Resources.OfType<MinistackResource>());
        Assert.Equal("ministack", resource.Name);
    }

    [Fact]
    public async Task CdkSampleAppHost_UsesExpectedContainerImage()
    {
        var builder = await fixture.CreateBuilderAsync();

        await using var app = await builder.BuildAsync();

        var resource = Assert.Single(builder.Resources.OfType<MinistackResource>());

        var imageAnnotation = Assert.Single(resource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(MinistackContainerImageTags.Registry, imageAnnotation.Registry);
        Assert.Equal(MinistackContainerImageTags.Image, imageAnnotation.Image);
        Assert.Equal(MinistackContainerImageTags.Tag, imageAnnotation.Tag);
    }

    [Fact]
    public async Task CdkSampleAppHost_HasCdkBootstrapConfigured()
    {
        var builder = await fixture.CreateBuilderAsync();

        await using var app = await builder.BuildAsync();

        var resource = Assert.Single(builder.Resources.OfType<MinistackResource>());
        Assert.True(resource.Annotations.OfType<CdkBootstrapAnnotation>().Any());
    }

    [Fact]
    public async Task CdkSampleAppHost_CdkBootstrapAnnotation_HasExpectedQualifier()
    {
        var builder = await fixture.CreateBuilderAsync();

        await using var app = await builder.BuildAsync();

        var resource = Assert.Single(builder.Resources.OfType<MinistackResource>());
        var annotation = Assert.Single(resource.Annotations.OfType<CdkBootstrapAnnotation>());
        Assert.Equal("myapp", annotation.Qualifier);
    }
}

public class CdkAspireTestingFixture
{
    public Task<IDistributedApplicationTestingBuilder> CreateBuilderAsync()
        => DistributedApplicationTestingBuilder.CreateAsync<Projects.McDoit_Aspire_Hosting_Ministack_Sample_Cdk_AppHost>();
}
