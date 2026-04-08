using Amazon;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using McDoit.Aspire.Hosting.Ministack;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
    .WithRegion(RegionEndpoint.USEast1);

var ministack = builder.AddMinistack(awsConfig)
    .WithCdkBootstrap("myapp")
    .WithStackport();

builder.AddAWSCDKStack("MyStack", app =>
    {
        var stack = new Stack(app, "MyStack");

        var bucket = new Bucket(stack, "MyBucket");

        var role = new Role(stack, "MyRole", new RoleProps
        {
            AssumedBy = new AccountRootPrincipal()
        });

        bucket.GrantRead(role);

        stack.ExportValue(role.RoleArn);
        stack.ExportValue(bucket.BucketName);

		return stack;
    })
    .WithReference(awsConfig)
	.WaitFor(ministack);

builder.Build().Run();
