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
    });
```

## Aspire usage

The way it works is by setting an application specific profile populated by Ministack endpoint as service endpoint  
Just referencing ```awsConfig``` will enable other resources to connect

## Repository structure

- `src/McDoit.Aspire.Hosting.Ministack` - package source
- `samples/McDoit.Aspire.Hosting.Ministack.Sample.AppHost` - runnable Aspire sample
- `tests/McDoit.Aspire.Hosting.Ministack.Tests` - unit + integration tests
