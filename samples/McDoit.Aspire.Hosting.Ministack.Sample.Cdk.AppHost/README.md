# CDK Sample AppHost (`samples/McDoit.Aspire.Hosting.Ministack.Sample.Cdk.AppHost`)

This sample shows how to:

- configure AWS settings with `AddAWSSDKConfig()`
- add `ministack` with `AddMinistack(...)`
- run CDK bootstrap against the local Ministack instance with `WithCdkBootstrap(...)`
- add the Stackport web UI with `WithStackport()`

## CDK bootstrap

`WithCdkBootstrap` runs `npx cdk bootstrap` against the Ministack endpoint when it becomes available.
An optional qualifier can be provided to scope the bootstrap stack (e.g. `"myapp"`).

## Run

From repository root:

```bash
dotnet run --project samples/McDoit.Aspire.Hosting.Ministack.Sample.Cdk.AppHost
```
