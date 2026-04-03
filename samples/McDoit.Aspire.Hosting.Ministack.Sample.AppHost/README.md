# Sample AppHost (`samples/McDoit.Aspire.Hosting.Ministack.Sample.AppHost`)

This sample shows how to:

- configure AWS settings with `AddAWSSDKConfig()`
- add `ministack` with `AddMinistack(...)`
- provision minimal AWS resources via `AddAWSCloudFormationTemplate(...)`

## Included CloudFormation resources

- `AWS::SQS::Queue`
- `AWS::SNS::Topic`
- `AWS::DynamoDB::Table`

## Run

From repository root:

```bash
dotnet run --project samples/McDoit.Aspire.Hosting.Ministack.Sample.AppHost
```

The sample also configures a container (`resource-inspector`) that consumes CloudFormation outputs via environment variables.
