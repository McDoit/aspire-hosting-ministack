using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.ECR.Model;
using Amazon.SimpleSystemsManagement.Model;
using McDoit.Aspire.Hosting.Ministack.Helpers;
using McDoit.Aspire.Hosting.Ministack.Tests.Fixture;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

/// <summary>
/// Live integration tests that start the CDK sample AppHost, allow <c>npx cdk bootstrap</c>
/// to run against Ministack, then assert that the expected bootstrap resources are present.
/// These tests require Docker and <c>npx</c>/<c>aws-cdk</c> to be available.
/// </summary>
[Collection("Sample CDK App collection")]
public class MinistackCdkBootstrapResourceTests(CdkBootstrapLiveFixture fixture)
{
    [Fact]
    public async Task CdkBootstrap_CreatesAssetsBucket()
    {
        var response = await fixture.S3Client.ListBucketsAsync(TestContext.Current.CancellationToken);

        Assert.Contains(response.Buckets, b => b.BucketName.StartsWith("cdk-myapp-assets-"));
    }

    [Fact]
    public async Task CdkBootstrap_CreatesContainerRegistry()
    {
        var response = await fixture.EcrClient.DescribeRepositoriesAsync(new DescribeRepositoriesRequest(), TestContext.Current.CancellationToken);

        Assert.Contains(response.Repositories, r => r.RepositoryName.StartsWith("cdk-myapp-container-assets-"));
    }

    [Fact]
    public async Task CdkBootstrap_CreatesToolkitStack()
    {
		var annotation = Assert.Single(fixture.MinistackResource.Annotations.OfType<CdkBootstrapAnnotation>());

		var response = await fixture.CloudFormationClient.DescribeStacksAsync(
            new DescribeStacksRequest { StackName = "CDKToolkit-" + annotation.Qualifier }, TestContext.Current.CancellationToken);

        var stack = Assert.Single(response.Stacks);
        Assert.Equal(StackStatus.CREATE_COMPLETE, stack.StackStatus);
    }

    [Fact]
    public async Task CdkBootstrap_CreatesVersionSsmParameter()
    {
        var response = await fixture.SsmClient.GetParameterAsync(
            new GetParameterRequest { Name = "/cdk-bootstrap/myapp/version" }, TestContext.Current.CancellationToken);

        Assert.NotNull(response.Parameter.Value);
    }
}
