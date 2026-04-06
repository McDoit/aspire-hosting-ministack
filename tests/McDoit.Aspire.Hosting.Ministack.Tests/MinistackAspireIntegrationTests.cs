using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using McDoit.Aspire.Hosting.Ministack.Resources;
using McDoit.Aspire.Hosting.Ministack.Helpers;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

public class MinistackAspireIntegrationTests(AspireTestingFixture fixture) : IClassFixture<AspireTestingFixture>
{
    [Fact]
    public async Task SampleAppHost_CreatesMinistackResource()
    {
        var builder = await fixture.CreateBuilderAsync();

        await using var app = await builder.BuildAsync();

        var resource = Assert.Single(builder.Resources.OfType<MinistackResource>());
        Assert.Equal("ministack", resource.Name);
	}

    [Fact]
    public async Task SampleAppHost_UsesConfigureContainerValuesForMinistack()
    {
        var builder = await fixture.CreateBuilderAsync();

        await using var app = await builder.BuildAsync();

        var resource = Assert.Single(builder.Resources.OfType<MinistackResource>());

        var imageAnnotation = Assert.Single(resource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(MinistackContainerImageTags.Registry, imageAnnotation.Registry);
        Assert.Equal(MinistackContainerImageTags.Image, imageAnnotation.Image);
        Assert.Equal(MinistackContainerImageTags.Tag, imageAnnotation.Tag);

        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>(), e => e.Name == MinistackResource.HttpEndpointName);
        Assert.Equal(4566, endpoint.TargetPort);
    }
}

public class AspireTestingFixture
{
    public Task<IDistributedApplicationTestingBuilder> CreateBuilderAsync()
        => DistributedApplicationTestingBuilder.CreateAsync<Projects.McDoit_Aspire_Hosting_Ministack_Sample_CloudFormation_AppHost>();
}
