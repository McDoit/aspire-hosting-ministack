using McDoit.Aspire.Hosting.Ministack.Resources;
using McDoit.Aspire.Hosting.Ministack.Helpers;
using McDoit.Aspire.Hosting.Ministack.Tests.Fixture;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

[Collection("Sample CDK App collection")]
[Trait("Category", "LiveIntegration")]
public class MinistackCdkAspireIntegrationTests(CdkBootstrapLiveFixture fixture)
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
