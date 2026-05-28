# Employee Series Management Service

Evaluation solution for SNCB (Belgian Train) implementing the provided ERD with **ASP.NET Core**, **Entity Framework Core**, **SQL Server**, layered architecture, REST API, Blazor WebAssembly UI, and unit tests.

## Solution structure

| Project | Role |
|---------|------|
| `EmployeeSeriesManagement.Domain` | Entities and enums |
| `EmployeeSeriesManagement.Application` | DTOs, business services, validation |
| `EmployeeSeriesManagement.Infrastructure` | EF Core `DbContext`, repositories, migrations, seed data |
| `EmployeeSeriesManagement.Api` | REST API controllers |
| `EmployeeSeriesManagement.Web` | Blazor WebAssembly UI |
| `EmployeeSeriesManagement.Tests` | xUnit tests |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or .NET 9+ with EF Core 9 packages if you retarget)

## Run with Docker (recommended)

The compose stack is split into one file per component, plus a root file that includes them all. Pick the granularity you need.

### Run everything (one command)

```bash
docker compose up --build
```

Open **http://localhost:8080**. The `web` service (nginx) serves the published Blazor WASM at `/` and reverse-proxies `/api/*` to the API container over the compose network — single host port, no CORS. The API exports OpenTelemetry traces to the collector, which forwards them to Honeycomb (if `HONEYCOMB_API_KEY` is set in `.env`).

### Run a single component

```bash
docker compose -f docker/db/docker-compose.yml         up -d   # SQL Server only
docker compose -f docker/honeycomb/docker-compose.yml  up -d   # OTel collector only
docker compose -f docker/app/docker-compose.yml        up --build  # api + web only (assumes db + collector reachable)
```

All sub-composes attach to the same named network (`esm-net`), so any combination works. You can chain files when you want a custom subset:

```bash
# DB + app, no Honeycomb collector
docker compose -f docker/db/docker-compose.yml -f docker/app/docker-compose.yml up --build
```

### Layout

| File | Services |
|------|----------|
| [docker-compose.yml](docker-compose.yml) | `include:` of the three below + `depends_on` wiring |
| [docker/db/docker-compose.yml](docker/db/docker-compose.yml) | `db` (SQL Server 2022, volume `mssql-data`, host port 1433) |
| [docker/honeycomb/docker-compose.yml](docker/honeycomb/docker-compose.yml) | `otel-collector` (OTLP gRPC `:4317`, OTLP HTTP `:4318`, health `:13133`) |
| [docker/app/docker-compose.yml](docker/app/docker-compose.yml) | `api` (ASP.NET Core, container port 8080) + `web` (nginx, host port 8080) |
| [docker/honeycomb/collector-config.yaml](docker/honeycomb/collector-config.yaml) | Collector pipeline: OTLP receiver → batch → Honeycomb exporter |

### Environment

Compose reads variables from `.env` at the repo root (gitignored). Create it with:

```
MSSQL_SA_PASSWORD=Dev!Strong@Password123
HONEYCOMB_API_KEY=hcaik_xxx...   # leave empty to skip Honeycomb export
HONEYCOMB_DATASET=                # Honeycomb Classic only
```

### Common ops

- `docker compose down` — stop containers, **keep** the SQL Server volume.
- `docker compose down -v` — stop and **wipe** the volume; next `up` re-applies migrations and re-seeds.
- `docker compose logs -f api` — tail API logs.
- `docker compose logs -f otel-collector` — see incoming spans (the collector's `debug` exporter prints to stdout).

Requires Docker Desktop / Docker Engine with Compose v2.20+ (for `include:` support).

## Run the .NET apps natively, DB in Docker

If you'd rather run the API and the Blazor app via `dotnet run` (faster inner loop, debugger attaches directly), keep just the database in a container:

```bash
docker compose up -d db          # only the SQL Server service, exposed on localhost:1433
```

The existing compose already publishes `1433:1433` and the API's [appsettings.json](src/EmployeeSeriesManagement.Api/appsettings.json) connection string already targets `localhost,1433`, so no extra config is needed. Then in two terminals:

**Terminal 1 – API**

```bash
cd src/EmployeeSeriesManagement.Api
dotnet run
```

API: `https://localhost:7280`. Migrations + demo seed run automatically against the SQL Server container on first start.

**Terminal 2 – Blazor UI**

```bash
cd src/EmployeeSeriesManagement.Web
dotnet run
```

Open the URL shown in the console (typically `https://localhost:7xxx`). Use the **Work city** dropdown; **Brussels** is pre-selected and shows two employees’ personal addresses.

## REST endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/employees/{id}/addresses` | All addresses for an employee |
| `GET` | `/api/employees/personal-addresses?workCity=Brussels` | Personal addresses where work city matches |
| `GET` | `/api/employees/work-cities` | Distinct work cities (dropdown source) |
| `GET` | `/api/employees/{id}/series?startDate=2026-01-01&endDate=2026-12-31` | Series overlapping a period |
| `POST` | `/api/employees/{id}/series` | Assign series (`{ "seriesCode": 502, "startDate": "...", "endDate": "..." }`) |

### Examples

Examples below use the `dotnet run` URL (`https://localhost:7280`). With Docker, swap the base to `http://localhost:8080` and drop `-k`.

```bash
curl -k "https://localhost:7280/api/employees/1001/addresses"
curl -k "https://localhost:7280/api/employees/personal-addresses?workCity=Brussels"
curl -k "https://localhost:7280/api/employees/1001/series?startDate=2026-06-01&endDate=2026-12-31"
curl -k -X POST "https://localhost:7280/api/employees/1002/series" \
  -H "Content-Type: application/json" \
  -d "{\"seriesCode\":502,\"startDate\":\"2026-04-01\",\"endDate\":\"2026-10-31\"}"
```

## Database

- **SQL Server** (Developer edition image, code-first EF Core migrations under `Infrastructure/Data/Migrations`).
- Connection string in `ConnectionStrings:DefaultConnection` (`appsettings.json` for `dotnet run`; injected via env var from `.env` in Docker).
- Tests use Testcontainers (`Testcontainers.MsSql`) to spin up a real SQL Server per test fixture — so `dotnet test` requires Docker to be running. The `Database:UseEnsureCreated=true` config flag tells the API to skip migration replay and create the schema from the EF model directly, which is faster against the empty test container.

Demo seed creates 3 employees, shared Brussels office work address, personal addresses, and sample series.

## Tests

```bash
dotnet test
```

## Logging

Structured logging via `ILogger<T>` in the API controller and `EmployeeService` (Information for operations, Warning for not-found).

## Observability — OpenTelemetry → Honeycomb

The API emits OpenTelemetry traces with three layers of spans per request:

1. **ASP.NET Core** auto-instrumented incoming HTTP span (controller, route, status).
2. **Application** custom span `EmployeeService.<Operation>` with business tags (`app.employee.external_id`, `app.series.code`, `app.work_city`, `app.period.start/end`, `app.result.count`, `app.outcome`).
3. **Infrastructure** custom span `EmployeeRepository.<Operation>` + **SqlClient** span with the executed SQL command.

Custom `ActivitySource`s: `EmployeeSeriesManagement.Application`, `EmployeeSeriesManagement.Infrastructure` ([ApplicationTelemetry.cs](src/EmployeeSeriesManagement.Application/Diagnostics/ApplicationTelemetry.cs), [InfrastructureTelemetry.cs](src/EmployeeSeriesManagement.Infrastructure/Diagnostics/InfrastructureTelemetry.cs)). Pipeline wired in [Program.cs](src/EmployeeSeriesManagement.Api/Program.cs).

### Local pipeline to Honeycomb

Honeycomb itself is SaaS — there is no self-hosted backend image. The repo ships an **OpenTelemetry Collector** container ([docker/honeycomb/docker-compose.yml](docker/honeycomb/docker-compose.yml), config in [docker/honeycomb/collector-config.yaml](docker/honeycomb/collector-config.yaml)) that receives OTLP from the API and forwards it to `api.honeycomb.io`.

**One-time setup:**

1. Grab a Honeycomb ingest key from https://ui.honeycomb.io → Account → Team settings → API keys.
2. Add `HONEYCOMB_API_KEY=...` to `.env` at the repo root (`.env` is gitignored).

**Run only the collector** (e.g. when the API runs natively via `dotnet run`):

```bash
docker compose -f docker/honeycomb/docker-compose.yml up -d
```

The API picks up `OpenTelemetry:OtlpEndpoint=http://localhost:4317` from [appsettings.Development.json](src/EmployeeSeriesManagement.Api/appsettings.Development.json) (native `dotnet run`) or the `OpenTelemetry__OtlpEndpoint=http://otel-collector:4317` env var (Docker). Spans land in your Honeycomb workspace under a dataset named after the resource's `service.name` (`EmployeeSeriesManagement.Api`).

Without an API key, the collector still runs but Honeycomb export returns 401; the `debug` exporter inside the collector logs incoming spans to stdout so you can verify the pipeline. View with `docker compose logs -f otel-collector`.

### Without Honeycomb

If you don't have a Honeycomb account, three options:

- **Console exporter only** — set `OpenTelemetry__OtlpEndpoint=` (empty) and the API prints spans to stdout. Already enabled in Development.
- **Swap the collector exporter** for `jaeger` / `zipkin` / `prometheus` by editing [docker/honeycomb/collector-config.yaml](docker/honeycomb/collector-config.yaml) — same receiver, different downstream.
- **.NET Aspire dashboard** — point `OpenTelemetry__OtlpEndpoint` directly at it (`http://localhost:18889`) and skip the collector entirely.

## Security approach (design notes)

For production SNCB/internal use, endpoints would be protected as follows:

1. **Authentication** – OAuth2/OIDC (e.g. Azure AD / Entra ID) with JWT bearer tokens on the API; Blazor WASM uses MSAL to acquire tokens and attach them to `HttpClient`.
2. **Authorization** – Role/policy-based (`Employee.Read`, `Employee.Write`, `Series.Assign`) on each action; read-only HR viewers vs. planners who can assign series.
3. **Transport** – HTTPS only, HSTS, no sensitive data in query strings where avoidable.
4. **Input validation** – Server-side validation (already in `CreateEmployeeSeriesValidator`); model binding limits; parameterized EF queries (default).
5. **Data protection** – Personal addresses and profile images are PII: encrypt at rest, audit access, minimize fields in API responses, rate limiting on list endpoints.
6. **API surface** – Optional API gateway (APIM) with subscription keys or mTLS for B2B; CORS restricted to known Blazor origins (configured in `Program.cs`).

This exercise ships **without auth** so evaluators can call the API directly.

## ERD mapping

- `Employees`, `Addresses`, `AddressType`, `EmployeesAddresses`, `Series`, `EmployeesSeries`, `EmployeesIdCards` tables match the diagram.
- Many-to-many: `EmployeesAddresses`, `EmployeesSeries` (with assignment `StartDate` / `EndDate`).
- `Language` stored as `FR` / `NL` enum.
