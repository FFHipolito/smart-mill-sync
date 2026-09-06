# Production Security Configuration

Smart Mill Sync fails closed outside `Development`. A production deployment must provide authentication, CORS, database and secret configuration before the API starts.

## Required Settings

Use environment variables or the deployment platform's secret store:

```text
ApiSecurity__RequireAuthentication=true
ApiSecurity__Authority=https://your-identity-provider.example.com
ApiSecurity__Audience=smart-mill-sync
Cors__AllowedOrigins__0=https://mill.example.com
ConnectionStrings__SmartMillDb=Host=...;Database=...;Username=...;Password=...
IndustrialAgent__ApiKey=<Google AI Studio authorization key>
```

`GOOGLE_AI_API_KEY` can be used instead of `IndustrialAgent__ApiKey` and takes precedence.

For local development, store the Gemini credential outside source control:

```bash
dotnet user-secrets set "IndustrialAgent:ApiKey" "YOUR_KEY" \
  --project src/SmartMillSync.Api/SmartMillSync.Api.csproj
```

## Reverse Proxy

Only trust known reverse proxies. Add one entry per proxy address:

```text
ForwardedHeaders__KnownProxies__0=10.0.0.10
```

Do not accept forwarded headers from arbitrary networks. Configure the production hostname through `AllowedHosts` as well.

## API And Realtime Access

- Controllers and `/hubs/mill-sync` use the global JWT fallback policy in production.
- `/health/live` remains anonymous and unlimited for process liveness probes.
- `/health/ready` remains anonymous for orchestrators but is rate limited and checks PostgreSQL.
- Swagger UI is available only in `Development`; publish the generated OpenAPI JSON as a build artifact for external consumers.
- Never put the Gemini key, database password or an API credential in the Blazor WebAssembly project.

## Limits

- Agent chat: 6 requests/minute per authenticated subject or source IP, 64 KB body, 2,000-character message, 20 history entries.
- Delivery writes: 20 requests/minute per subject/IP, 16 KB body.
- Reads and SignalR negotiation: 120 requests/minute per subject/IP.
- Readiness checks: 30 requests/minute per source IP.
