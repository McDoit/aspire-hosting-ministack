# Test project (`tests/McDoit.Aspire.Hosting.Ministack.Tests`)

This project contains:

- unit tests for core resource behavior (`MinistackResource`, image tag defaults)
- unit tests for `WithStackport` (container image, endpoint, manifest exclusion)
- integration tests using `DistributedApplicationTestingBuilder`
- integration tests based on the CloudFormation sample AppHost

## Run tests

From repository root:

```bash
dotnet test tests/McDoit.Aspire.Hosting.Ministack.Tests
```
