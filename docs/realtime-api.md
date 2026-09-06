# Smart Mill Sync Realtime API

## Connection

- Hub URL: `/hubs/mill-sync`
- Protocol: ASP.NET Core SignalR JSON
- Transports: WebSockets with SignalR fallback transports
- Client behavior: automatic reconnect is enabled in the Blazor client
- Rate limit: the `reads` policy applies to hub negotiation and connections
- Authentication: when `ApiSecurity:RequireAuthentication` is enabled, the global JWT fallback policy protects the hub
- Production configuration: set `ApiSecurity:Authority`, `ApiSecurity:Audience`, and at least one `Cors:AllowedOrigins` entry

The hub exposes no client-to-server business methods. It only broadcasts typed operational events.

## Server Events

### `DeliveryUpdated`

Payload: `WoodDeliveryResponse`

Published immediately after a wood delivery is persisted. The payload includes truck plate, origin, species, weights, moisture, status, thermal compensation, extra GN volume and alert level.

### `EnergyBalanceUpdated`

Payload: `EnergyBalanceSummaryDto`

Evaluated by the thermal worker every 30 seconds. The event is broadcast when estimated additional GN is greater than zero.

## Enum Encoding

Enums are encoded as their numeric contract values:

- `DeliveryStatus`: `1=InTransit`, `2=ArrivedAtGate`, `3=Weighed`, `4=Unloading`, `5=Completed`, `6=Rejected`
- `AlertLevel`: `1=Normal`, `2=Moderate`, `3=High`

## Security Notes

- Production deployments must configure an OIDC provider and provide valid access tokens to the API and SignalR client.
- Browser origins must be listed explicitly in `Cors:AllowedOrigins`.
- Never embed service credentials or the Gemini API key in a SignalR client.
