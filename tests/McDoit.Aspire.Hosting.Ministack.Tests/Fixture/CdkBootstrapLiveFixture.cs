using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.ECR;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Aspire.Hosting.Testing;
using McDoit.Aspire.Hosting.Ministack.Helpers;
using McDoit.Aspire.Hosting.Ministack.Resources;

namespace McDoit.Aspire.Hosting.Ministack.Tests.Fixture;

public class CdkBootstrapLiveFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public IAmazonS3 S3Client { get; private set; } = null!;
    public IAmazonECR EcrClient { get; private set; } = null!;
    public IAmazonCloudFormation CloudFormationClient { get; private set; } = null!;
    public IAmazonSimpleSystemsManagement SsmClient { get; private set; } = null!;

    public IDistributedApplicationTestingBuilder Builder { get; private set; } = null!;

    public MinistackResource MinistackResource { get; private set; } = null!;

	public async ValueTask InitializeAsync()
    {
        // Guard against the app or CDK bootstrap hanging indefinitely.
        var startupTimeout = TimeSpan.FromMinutes(GetBootstrapTimeout().TotalMinutes + 5);
        using var startupCts = new CancellationTokenSource(startupTimeout);

		Builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.McDoit_Aspire_Hosting_Ministack_Sample_Cdk_AppHost>();
        _app = await Builder.BuildAsync();
        await _app.StartAsync(startupCts.Token);

        MinistackResource = Builder.Resources.OfType<MinistackResource>().FirstOrDefault(f => f != null)
            ?? throw new System.InvalidOperationException("The sample CDK AppHost did not create a MinistackResource as expected.");

        Environment.SetEnvironmentVariable("AWS_PROFILE", MinistackResource.ProfileName);

        S3Client = new AmazonS3Client(new AmazonS3Config
        {
			ForcePathStyle = true
        });

        EcrClient = new AmazonECRClient();

        CloudFormationClient = new AmazonCloudFormationClient();

        SsmClient = new AmazonSimpleSystemsManagementClient();

        await WaitForCdkBootstrapAsync(MinistackResource.Annotations.OfType<CdkBootstrapAnnotation>().First(), startupCts.Token);
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

    private async Task WaitForCdkBootstrapAsync(CdkBootstrapAnnotation annotation, CancellationToken cancellationToken = default)
    {
        var timeout = GetBootstrapTimeout();
        var pollInterval = TimeSpan.FromSeconds(5);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await CloudFormationClient.DescribeStacksAsync(
                    new DescribeStacksRequest { StackName = "CDKToolkit-" + annotation.Qualifier }, cancellationToken);

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
            catch (AmazonCloudFormationException exc)
            {
                // Stack does not exist yet; keep polling.
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        throw new TimeoutException(
            $"CDK bootstrap did not complete within {timeout.TotalMinutes} minutes. " +
            "The CDKToolkit CloudFormation stack did not reach CREATE_COMPLETE.");
    }

    public async ValueTask DisposeAsync()
    {
        S3Client?.Dispose();
        EcrClient?.Dispose();
        CloudFormationClient?.Dispose();
        SsmClient?.Dispose();

        if (_app is not null)
            await _app.DisposeAsync();
    }
}
