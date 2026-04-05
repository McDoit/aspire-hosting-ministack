# McDoit.Aspire.Hosting.Ministack

`McDoit.Aspire.Hosting.Ministack` adds an Aspire resource for running [Ministack](https://ministack.org/) (Local AWS emulator) in an AppHost.

## What this package provides

- `AddMinistack(...)` extension for `IDistributedApplicationBuilder`
- HTTP endpoint + health check wiring for the `ministack` container
- AWS profile bootstrapping for local development credentials
- Container customization via `MinistackContainerOptions`

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

        // Use an external Redis instance for state persistence across container restarts
        options.RedisHost = "redis-host";

        // Override the AWS account ID (defaults to 000000000000 inside the container)
        options.AccountId = "123456789012";
    });
```

## Container options reference

| Option | Environment variable | Default | Description |
|--------|---------------------|---------|-------------|
| `Registry` | — | `docker.io` | Container registry |
| `Image` | — | `nahuelnucera/ministack` | Container image |
| `Tag` | — | `latest` | Image tag |
| `Port` | — | *(proxied)* | Host port; when set, proxying is disabled |
| `Lifetime` | `PERSIST_STATE`, `S3_PERSIST` | `Session` | `Persistent` enables file-based state persistence |
| `RedisHost` | `REDIS_HOST` | *(not set)* | Redis host for state persistence via Redis |
| `AccountId` | `MINISTACK_ACCOUNT_ID` | `000000000000` | Custom AWS account ID |

## Dev-only resource

The Ministack resource is automatically excluded from the Aspire publish manifest
(`ExcludeFromManifest`). It is intended for local development only and will not be
included when publishing or deploying the AppHost.

## Aspire usage

The way it works is by setting an application specific profile populated by Ministack endpoint as service endpoint  
Just referencing ```awsConfig``` will enable other resources to connect

## Repository structure

- `src/McDoit.Aspire.Hosting.Ministack` - package source
- `samples/McDoit.Aspire.Hosting.Ministack.Sample.AppHost` - runnable Aspire sample
- `tests/McDoit.Aspire.Hosting.Ministack.Tests` - unit + integration tests
