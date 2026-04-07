using Amazon;
using McDoit.Aspire.Hosting.Ministack;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
    .WithRegion(RegionEndpoint.USEast1);

var ministack = builder.AddMinistack(awsConfig)
    .WithStackport();

var awsResources = builder.AddAWSCloudFormationTemplate(
        "sample-dev-resources",
        "cloudformation/app-resources.template.json")
    .WithParameter("QueueName", "sample-events-queue")
    .WithParameter("TopicName", "sample-events-topic")
    .WithParameter("TableName", "sample-items-table")
	.WithReference(awsConfig)
	.WaitFor(ministack);

builder.AddContainer("resource-inspector", "alpine")
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c", "while true; do sleep 30; done")
    .WithReference(ministack)
    .WithReference(awsResources)
    .WithEnvironment("SAMPLE_QUEUE_URL", awsResources.GetOutput("SampleQueueUrl"))
    .WithEnvironment("SAMPLE_TOPIC_ARN", awsResources.GetOutput("SampleTopicArn"))
    .WithEnvironment("SAMPLE_TABLE_NAME", awsResources.GetOutput("SampleTableName"));

builder.Build().Run();

