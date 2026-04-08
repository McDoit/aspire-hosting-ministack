using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.ECR;
using Amazon.ECR.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Aspire.Hosting.Testing;

namespace McDoit.Aspire.Hosting.Ministack.Tests;

/// <summary>
/// Live integration tests that start the CDK sample AppHost, allow <c>npx cdk bootstrap</c>
/// to run against Ministack, then assert that the expected bootstrap resources are present.
/// These tests require Docker and <c>npx</c>/<c>aws-cdk</c> to be available.
/// </summary>
public class MinistackCdkBootstrapResourceTests(CdkBootstrapLiveFixture fixture)
    : IClassFixture<CdkBootstrapLiveFixture>
{
    [Fact]
    public async Task CdkBootstrap_CreatesAssetsBucket()
    {
        var response = await fixture.S3Client.ListBucketsAsync();

        Assert.Contains(response.Buckets, b => b.BucketName.StartsWith("cdk-myapp-assets-"));
    }

    [Fact]
    public async Task CdkBootstrap_CreatesContainerRegistry()
    {
        var response = await fixture.EcrClient.DescribeRepositoriesAsync(new DescribeRepositoriesRequest());

        Assert.Contains(response.Repositories, r => r.RepositoryName.StartsWith("cdk-myapp-container-assets-"));
    }

    [Fact]
    public async Task CdkBootstrap_CreatesToolkitStack()
    {
        var response = await fixture.CloudFormationClient.DescribeStacksAsync(
            new DescribeStacksRequest { StackName = "CDKToolkit" });

        var stack = Assert.Single(response.Stacks);
        Assert.Equal(StackStatus.CREATE_COMPLETE, stack.StackStatus);
    }

    [Fact]
    public async Task CdkBootstrap_CreatesVersionSsmParameter()
    {
        var response = await fixture.SsmClient.GetParameterAsync(
            new GetParameterRequest { Name = "/cdk-bootstrap/myapp/version" });

        Assert.NotNull(response.Parameter.Value);
    }
}

//[assembly: AssemblyFixture(typeof(DatabaseFixture))]
public class CdkBootstrapLiveFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public IAmazonS3 S3Client { get; private set; } = null!;
    public IAmazonECR EcrClient { get; private set; } = null!;
    public IAmazonCloudFormation CloudFormationClient { get; private set; } = null!;
    public IAmazonSimpleSystemsManagement SsmClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Guard against the app or CDK bootstrap hanging indefinitely.
        var startupTimeout = TimeSpan.FromMinutes(GetBootstrapTimeout().TotalMinutes + 5);
        using var startupCts = new CancellationTokenSource(startupTimeout);

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.McDoit_Aspire_Hosting_Ministack_Sample_Cdk_AppHost>();
        _app = await builder.BuildAsync();
        await _app.StartAsync(startupCts.Token);

        var connectionString = await _app.GetConnectionStringAsync("ministack")
            ?? throw new System.InvalidOperationException("Could not retrieve the Ministack connection string.");

        var credentials = new BasicAWSCredentials("ministack", "ministack");

        S3Client = new AmazonS3Client(credentials, new AmazonS3Config
        {
            ServiceURL = connectionString,
            ForcePathStyle = true,
        });

        EcrClient = new AmazonECRClient(credentials, new AmazonECRConfig
        {
            ServiceURL = connectionString,
        });

        CloudFormationClient = new AmazonCloudFormationClient(credentials, new AmazonCloudFormationConfig
        {
            ServiceURL = connectionString,
        });

        SsmClient = new AmazonSimpleSystemsManagementClient(credentials, new AmazonSimpleSystemsManagementConfig
        {
            ServiceURL = connectionString,
        });

        await WaitForCdkBootstrapAsync(startupCts.Token);
    }

    private static TimeSpan GetBootstrapTimeout()
    {
        if (int.TryParse(
                Environment.GetEnvironmentVariable("CDK_BOOTSTRAP_TIMEOUT_MINUTES"),
                out var minutes) && minutes > 0)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        return TimeSpan.FromMinutes(5);
    }

    private static readonly StackStatus[] _failureStatuses =
    [
        StackStatus.CREATE_FAILED,
        StackStatus.ROLLBACK_COMPLETE,
        StackStatus.ROLLBACK_FAILED,
        StackStatus.DELETE_COMPLETE,
        StackStatus.DELETE_FAILED,
    ];

    private async Task WaitForCdkBootstrapAsync(CancellationToken cancellationToken = default)
    {
        var timeout = GetBootstrapTimeout();
        var pollInterval = TimeSpan.FromSeconds(5);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await CloudFormationClient.DescribeStacksAsync(
                    new DescribeStacksRequest { StackName = "CDKToolkit" }, cancellationToken);

                var stack = response.Stacks.FirstOrDefault();
                if (stack is not null)
                {
                    if (stack.StackStatus == StackStatus.CREATE_COMPLETE)
                        return;

                    if (_failureStatuses.Contains(stack.StackStatus))
                        throw new System.InvalidOperationException(
                            $"CDK bootstrap failed. The CDKToolkit CloudFormation stack reached a terminal failure state: {stack.StackStatus}.");
                }
            }
            catch (AmazonCloudFormationException)
            {
                // Stack does not exist yet; keep polling.
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        throw new TimeoutException(
            $"CDK bootstrap did not complete within {timeout.TotalMinutes} minutes. " +
            "The CDKToolkit CloudFormation stack did not reach CREATE_COMPLETE.");
    }

    public async Task DisposeAsync()
    {
        S3Client?.Dispose();
        EcrClient?.Dispose();
        CloudFormationClient?.Dispose();
        SsmClient?.Dispose();

        if (_app is not null)
            await _app.DisposeAsync();
    }
}
