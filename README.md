# McDoit.Aspire.Hosting.Ministack

[![NuGet](https://img.shields.io/nuget/v/McDoit.Aspire.Hosting.Ministack)](https://www.nuget.org/packages/McDoit.Aspire.Hosting.Ministack)

`McDoit.Aspire.Hosting.Ministack` adds an Aspire resource for running [Ministack](https://ministack.org/) (Local AWS emulator) in an AppHost.

## What this package provides

- `AddMinistack(...)` extension for `IDistributedApplicationBuilder`
- HTTP endpoint + health check wiring for the `ministack` container
- AWS profile bootstrapping for local development credentials
- Container customization via `MinistackContainerOptions`
- `WithStackport(...)` — adds the [Stackport](https://hub.docker.com/r/davireis/stackport) web UI for browsing local AWS resources
- `WithCdkBootstrap(...)` — runs `npx cdk bootstrap` against the local Ministack instance when it becomes available

## Install

```bash
dotnet add package McDoit.Aspire.Hosting.Ministack
```

## Quick usage

```csharp
using Amazon;
using Aspire.Hosting;
using McDoit.Aspire.Hosting.Ministack;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
    .WithRegion(RegionEndpoint.USEast1);

builder.AddMinistack(awsConfig);

builder.Build().Run();
```

## Configure container values

```csharp
builder.AddMinistack(
    awsConfig,
    configureContainer: options =>
    {
        options.Registry = "ghcr.io";
        options.Image = "acme/ministack";
        options.Tag = "2.3.4";
        options.Port = 14566;
        options.Lifetime = ContainerLifetime.Persistent;
    });
```

## Add Stackport web UI

[Stackport](https://hub.docker.com/r/davireis/stackport) is a universal AWS resource browser for local emulators.
Chain `WithStackport()` to add it alongside Ministack:

```csharp
builder.AddMinistack(awsConfig)
    .WithStackport();
```

An optional `port` parameter fixes the host port; omitting it lets Aspire assign one automatically.

## CDK bootstrap

`WithCdkBootstrap()` runs `npx cdk bootstrap` against the Ministack endpoint when it becomes available.
An optional qualifier scopes the bootstrap stack:

```csharp
builder.AddMinistack(awsConfig)
    .WithCdkBootstrap("myapp")
    .WithStackport();
```

## Container options reference

| Option | Environment variable | Default | Description |
|--------|---------------------|---------|-------------|
| `Registry` | — | `docker.io` | Container registry |
| `Image` | — | `nahuelnucera/ministack` | Container image |
| `Tag` | — | `latest` | Image tag |
| `Port` | — | *(proxied)* | Host port; when set, proxying is disabled |
| `Lifetime` | `PERSIST_STATE`, `S3_PERSIST` | `Session` | `Persistent` enables file-based state persistence |

## Dev-only resource

The Ministack resource is automatically excluded from the Aspire publish manifest
(`ExcludeFromManifest`). It is intended for local development only and will not be
included when publishing or deploying the AppHost.

## Aspire usage

The way it works is by setting an application specific profile populated by Ministack endpoint as service endpoint  
Just referencing ```awsConfig``` will enable other resources to connect

## Repository structure

- `src/McDoit.Aspire.Hosting.Ministack` - package source
- `samples/McDoit.Aspire.Hosting.Ministack.Sample.CloudFormation.AppHost` - runnable Aspire sample using CloudFormation
- `samples/McDoit.Aspire.Hosting.Ministack.Sample.Cdk.AppHost` - runnable Aspire sample using CDK bootstrap
- `tests/McDoit.Aspire.Hosting.Ministack.Tests` - unit + integration tests
