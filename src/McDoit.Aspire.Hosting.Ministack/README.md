# Source project (`src/McDoit.Aspire.Hosting.Ministack`)

This project contains the reusable Aspire hosting integration exposed through:

- `MinistackResource`
- `MinistackResourceBuilderExtensions.AddMinistack(...)`
- `MinistackResourceBuilderExtensions.WithStackport(...)`
- `MinistackResourceBuilderExtensions.WithCdkBootstrap(...)`
- `MinistackContainerOptions`

## Main behavior

- Adds a container resource for `ministack`
- Automatically excluded from the Aspire publish manifest (`ExcludeFromManifest`) — dev-only
- Configures image/registry/tag with defaults that can be overridden
- Adds an HTTP endpoint on target port `4566`
- Adds a health check (`/_ministack/health`)
- Configures an AWS credential profile pointing at the Ministack endpoint for use by the AWS SDK
- `WithStackport(...)` — adds the [Stackport](https://hub.docker.com/r/davireis/stackport) web UI container (target port `8080`, health check at `/api/health`)
- `WithCdkBootstrap(...)` — runs `npx cdk bootstrap` against the Ministack endpoint once it is ready; accepts an optional qualifier

## Defaults

- Registry: `docker.io`
- Image: `ministackorg/ministack`
- Tag: `latest`
