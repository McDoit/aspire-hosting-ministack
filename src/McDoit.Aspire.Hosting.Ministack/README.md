# Source project (`src/McDoit.Aspire.Hosting.Ministack`)

This project contains the reusable Aspire hosting integration exposed through:

- `MinistackResource`
- `MinistackResourceBuilderExtensions.AddMinistack(...)`
- `MinistackContainerOptions`

## Main behavior

- Adds a container resource for `ministack`
- Automatically excluded from the Aspire publish manifest (`ExcludeFromManifest`) — dev-only
- Configures image/registry/tag with defaults that can be overridden
- Adds an HTTP endpoint on target port `4566`
- Adds a health check (`/_ministack/health`)
- Configures AWS profile credentials for local emulator access

## Defaults

- Registry: `docker.io`
- Image: `nahuelnucera/ministack`
- Tag: `latest`
