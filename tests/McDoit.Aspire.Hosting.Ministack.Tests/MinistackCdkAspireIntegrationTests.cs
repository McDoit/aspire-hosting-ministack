using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using McDoit.Aspire.Hosting.Ministack.Resources;
using McDoit.Aspire.Hosting.Ministack.Helpers;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

public class MinistackCdkAspireIntegrationTests(CdkAspireTestingFixture fixture) : IClassFixture<CdkAspireTestingFixture>
{
    [Fact]
    public void CdkSampleAppHost_CreatesMinistackResource()
    {
        var resource = Assert.Single(fixture.Builder.Resources.OfType<MinistackResource>());
        Assert.Equal("ministack", resource.Name);
    }

    [Fact]
    public void CdkSampleAppHost_UsesExpectedContainerImage()
    {
        var resource = Assert.Single(fixture.Builder.Resources.OfType<MinistackResource>());

        var imageAnnotation = Assert.Single(resource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(MinistackContainerImageTags.Registry, imageAnnotation.Registry);
        Assert.Equal(MinistackContainerImageTags.Image, imageAnnotation.Image);
        Assert.Equal(MinistackContainerImageTags.Tag, imageAnnotation.Tag);
    }

    [Fact]
    public void CdkSampleAppHost_HasCdkBootstrapConfigured()
    {
        var resource = Assert.Single(fixture.Builder.Resources.OfType<MinistackResource>());
        Assert.True(resource.Annotations.OfType<CdkBootstrapAnnotation>().Any());
    }

    [Fact]
    public void CdkSampleAppHost_CdkBootstrapAnnotation_HasExpectedQualifier()
    {
        var resource = Assert.Single(fixture.Builder.Resources.OfType<MinistackResource>());
        var annotation = Assert.Single(resource.Annotations.OfType<CdkBootstrapAnnotation>());
        Assert.Equal("myapp", annotation.Qualifier);
    }
}


public class CdkAspireTestingFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public IDistributedApplicationTestingBuilder Builder { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.McDoit_Aspire_Hosting_Ministack_Sample_Cdk_AppHost>();
        _app = await Builder.BuildAsync();
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync();
    }
}
