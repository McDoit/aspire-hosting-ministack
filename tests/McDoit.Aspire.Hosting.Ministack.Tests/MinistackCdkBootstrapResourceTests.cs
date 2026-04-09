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
    private readonly string _qualifier = fixture.MinistackResource.Annotations.OfType<CdkBootstrapAnnotation>().First().Qualifier ?? CdkBootstrapAnnotation.DefaultQualifer;

	private string GenerateCdkBootstrapResourceName(string resourceType) =>
        $"cdk-{_qualifier}-{resourceType}-{fixture.MinistackResource.AccountId}-{fixture.MinistackResource.Region.SystemName}";

	[Fact]
    public async Task CdkBootstrap_CreatesAssetsBucket()
    {
        var response = fixture.S3Client.Paginators.ListBuckets(new Amazon.S3.Model.ListBucketsRequest());

        var buckets = response.Buckets.ToBlockingEnumerable();

        Assert.Contains(buckets, b => b.BucketName.Equals(GenerateCdkBootstrapResourceName("assets")));
    }

    [Fact]
    public async Task CdkBootstrap_CreatesContainerRegistry()
    {
		var response = fixture.EcrClient.Paginators.DescribeRepositories(new DescribeRepositoriesRequest());

        var repositories = response.Repositories.ToBlockingEnumerable();

        Assert.Contains(repositories, r => r.RepositoryName.Equals(GenerateCdkBootstrapResourceName("container-assets")));
	}

	[Fact]
	public async Task CdkBootstrap_CreatesIamRoles()
	{
		var response = fixture.IamClient.Paginators.ListRoles(new Amazon.IdentityManagement.Model.ListRolesRequest());

		var roles = response.Roles.ToBlockingEnumerable().ToArray();

		Assert.Contains(roles, r => r.RoleName.Equals(GenerateCdkBootstrapResourceName("deploy-role")));
		Assert.Contains(roles, r => r.RoleName.Equals(GenerateCdkBootstrapResourceName("cfn-exec-role")));
		Assert.Contains(roles, r => r.RoleName.Equals(GenerateCdkBootstrapResourceName("lookup-role")));
		Assert.Contains(roles, r => r.RoleName.Equals(GenerateCdkBootstrapResourceName("file-publishing-role")));
		Assert.Contains(roles, r => r.RoleName.Equals(GenerateCdkBootstrapResourceName("image-publishing-role")));
	}

	[Fact]
    public async Task CdkBootstrap_CreatesToolkitStack()
    {
		var annotation = Assert.Single(fixture.MinistackResource.Annotations.OfType<CdkBootstrapAnnotation>());

		var response = await fixture.CloudFormationClient.DescribeStacksAsync(
            new DescribeStacksRequest { StackName = string.IsNullOrWhiteSpace(annotation.Qualifier) ? "CDKToolkit" : "CDKToolkit-" + annotation.Qualifier });

        var stack = Assert.Single(response.Stacks);
        Assert.Equal(StackStatus.CREATE_COMPLETE, stack.StackStatus);
    }

    [Fact]
    public async Task CdkBootstrap_CreatesVersionSsmParameter()
    {
        var response = await fixture.SsmClient.GetParameterAsync(
            new GetParameterRequest { Name = $"/cdk-bootstrap/{_qualifier}/version" });

        Assert.NotNull(response.Parameter.Value);
    }
}
