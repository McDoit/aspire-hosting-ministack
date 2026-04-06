using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using McDoit.Aspire.Hosting.Ministack.Resources;
using McDoit.Aspire.Hosting.Ministack.Helpers;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

public class MinistackAspireIntegrationTests(AspireTestingFixture fixture) : IClassFixture<AspireTestingFixture>
{
    [Fact]
    public void SampleAppHost_CreatesMinistackResource()
    {
        var resource = Assert.Single(fixture.Builder.Resources.OfType<MinistackResource>());
        Assert.Equal("ministack", resource.Name);
	}

    [Fact]
    public void SampleAppHost_UsesConfigureContainerValuesForMinistack()
    {
        var resource = Assert.Single(fixture.Builder.Resources.OfType<MinistackResource>());

        var imageAnnotation = Assert.Single(resource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(MinistackContainerImageTags.Registry, imageAnnotation.Registry);
        Assert.Equal(MinistackContainerImageTags.Image, imageAnnotation.Image);
        Assert.Equal(MinistackContainerImageTags.Tag, imageAnnotation.Tag);

        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>(), e => e.Name == MinistackResource.HttpEndpointName);
        Assert.Equal(4566, endpoint.TargetPort);
    }
}

public class AspireTestingFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public IDistributedApplicationTestingBuilder Builder { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.McDoit_Aspire_Hosting_Ministack_Sample_CloudFormation_AppHost>();
        _app = await Builder.BuildAsync();
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync();
    }
}
