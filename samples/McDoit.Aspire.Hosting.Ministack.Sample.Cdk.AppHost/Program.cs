using Amazon;
using McDoit.Aspire.Hosting.Ministack;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
    .WithRegion(RegionEndpoint.USEast1);

builder.AddMinistack(awsConfig)
    .WithCdkBootstrap("myapp")
    .WithStackport();

builder.Build().Run();
