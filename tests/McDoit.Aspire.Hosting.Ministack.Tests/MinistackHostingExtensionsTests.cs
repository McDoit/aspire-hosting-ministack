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
}
