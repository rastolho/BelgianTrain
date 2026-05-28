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

- **Docker Desktop** (or Docker Engine + Compose v2.20+ for `include:` support).
- Optional, only for `dotnet run` / `dotnet test`: [.NET 10 SDK](https://dotnet.microsoft.com/download).

Tests use **Testcontainers**, which spins up a SQL Server image — Docker must be running even if you choose to run the apps natively.

---

## Run locally — full stack in Docker (recommended)

The whole solution (frontend + backend + database + OpenTelemetry collector + Jaeger UI) comes up with one command.

### 1. Clone and create `.env`

```bash
git clone https://github.com/rastolho/BelgianTrain.git
cd BelgianTrain
```

Create a `.env` file at the repo root (gitignored):

```
MSSQL_SA_PASSWORD=Dev!Strong@Password123
```

This password is read by both the `db` container (SA password) and the `api` container (connection string).

### 2. Bring up the stack

```bash
docker compose up --build
```

First run builds the API and Web images (~1–2 minutes), pulls SQL Server, OTel Collector, and Jaeger images, then starts everything. The API waits for the DB healthcheck before starting, then auto-applies EF Core migrations and seeds demo data (3 employees, Brussels + Antwerp work addresses, sample series).

Add `-d` to detach. To rebuild only after code changes: `docker compose up --build -d`.

### 3. URLs

Everything you need is on **localhost**:

| URL | What it is | Used for |
|-----|------------|----------|
| **http://localhost:8080** | Blazor WASM UI (served by nginx) | Open this in the browser — the main app |
| **http://localhost:8080/api/...** | REST API (proxied through nginx to the API container) | Same origin as the UI, no CORS |
| **http://localhost:16686** | Jaeger UI | Inspect every request's trace waterfall |
| **localhost:1433** | SQL Server | Connect with SSMS / Azure Data Studio / `sqlcmd` (user `sa`, password from `.env`) |
| **http://localhost:13133** | OTel Collector health | `curl` returns 200 OK when collector is ready |
| **localhost:4317 / 4318** | OTel Collector OTLP receivers | Only needed if you point a native `dotnet run` API at the collector — see below |

The API container's port `8080` is **not** exposed directly. All API traffic from outside the docker network goes through nginx (`web` service) on `localhost:8080`, which proxies `/api/*` to the API container. Same origin → no CORS configuration in the browser.

### 4. First steps in the UI

1. Open **http://localhost:8080** → home page (`/personal-addresses`) loads with an empty work-city dropdown.
2. Click the dropdown → pick **Brussels** → table shows the two employees (Dupont, Janssens) whose office is in Brussels, with their home addresses (Ixelles, Schaerbeek).
3. Use the left nav to try the other pages:
   - **Employee addresses** — enter `1001` to see both work + personal addresses for that employee.
   - **Employee series** — enter `1001`, pick a date range, see series assigned in that period.
   - **Assign series (internal)** — POST form for the back-office endpoint; try `employeeId=1002`, `seriesCode=501`, any future date range.

### 5. Inspecting traces

Open **http://localhost:16686**:

1. In the *Service* dropdown pick **`EmployeeSeriesManagement.Api`**.
2. Click **Find Traces**.
3. Click any trace to see the span waterfall:
   ```
   GET api/Employees/{id}/addresses   ← ASP.NET Core (root, http.* tags)
     └─ EmployeeService.GetEmployeeAddresses          ← Application layer (app.outcome, app.result.count)
        └─ EmployeeRepository.EmployeeExists          ← Infrastructure layer
        └─ EmployeeRepository.GetEmployeeAddresses
           └─ Microsoft.Data.SqlClient.Execute        ← SQL command auto-instrumented
   ```
4. Click any span → side panel shows all custom tags (`app.employee.external_id`, `app.series.code`, `app.work_city`, `app.period.start/end`, `app.result.count`, `app.outcome=ok|not_found|conflict|validation_error|created`).

You can also tail span counts in the collector logs without opening the UI:

```bash
docker compose logs -f otel-collector
```

### 6. Sanity check via curl

```bash
curl http://localhost:8080/api/employees/work-cities
# → ["Antwerp","Brussels"]

curl http://localhost:8080/api/employees/1001/addresses
# → [{"id":1,"addressType":"Work","city":"Brussels",...}, {"id":3,"addressType":"Personal","city":"Ixelles",...}]

curl "http://localhost:8080/api/employees/personal-addresses?workCity=Brussels"
# → 2 employees (Dupont/Ixelles, Janssens/Schaerbeek)

curl -X POST http://localhost:8080/api/employees/1002/series \
  -H "Content-Type: application/json" \
  -d '{"seriesCode":501,"startDate":"2026-04-15","endDate":"2026-05-15"}'
# → 201 Created
```

Each call produces a fresh trace in Jaeger.

### 7. Stop / cleanup

```bash
docker compose down       # stop containers, keep SQL data volume
docker compose down -v    # stop and wipe the SQL volume (next `up` re-seeds)
```

---

## Run one component at a time

The compose stack is split into one file per component, joined by the root `docker-compose.yml` via `include:` (Compose v2.20+). You can run any subset:

```bash
docker compose -f docker/db/docker-compose.yml             up -d  # SQL Server only — localhost:1433
docker compose -f docker/otel-collector/docker-compose.yml up -d  # Collector only — localhost:4317/4318/13133
docker compose -f docker/jaeger/docker-compose.yml         up -d  # Jaeger UI only — localhost:16686
docker compose -f docker/app/docker-compose.yml            up --build  # api + web only — localhost:8080
```

All sub-composes attach to the same named network (`esm-net`), so any combination works. Chain `-f` flags for a custom subset:

```bash
# DB + app, no tracing
docker compose -f docker/db/docker-compose.yml -f docker/app/docker-compose.yml up --build
```

### File layout

| File | Services |
|------|----------|
| [docker-compose.yml](docker-compose.yml) | `include:` of the four below + `depends_on` wiring |
| [docker/db/docker-compose.yml](docker/db/docker-compose.yml) | `db` (SQL Server 2022, volume `mssql-data`, host port 1433) |
| [docker/otel-collector/docker-compose.yml](docker/otel-collector/docker-compose.yml) | `otel-collector` (OTLP gRPC `:4317`, OTLP HTTP `:4318`, health `:13133`) |
| [docker/jaeger/docker-compose.yml](docker/jaeger/docker-compose.yml) | `jaeger` all-in-one (UI on host port `:16686`) |
| [docker/app/docker-compose.yml](docker/app/docker-compose.yml) | `api` (ASP.NET Core, container port 8080) + `web` (nginx, host port 8080) |
| [docker/otel-collector/collector-config.yaml](docker/otel-collector/collector-config.yaml) | Collector pipeline: OTLP receiver → batch → Jaeger + debug |

---

## Run the .NET apps natively (debugger attached), DB in Docker

If you'd rather run the API and the Blazor app via `dotnet run` for a faster inner loop / Visual Studio debugger, keep just the database (and optionally the tracing stack) in containers:

```bash
docker compose -f docker/db/docker-compose.yml             up -d  # SQL Server → localhost:1433
docker compose -f docker/otel-collector/docker-compose.yml up -d  # optional — only if you want traces
docker compose -f docker/jaeger/docker-compose.yml         up -d  # optional — only if you want Jaeger UI
```

Then in two terminals:

**Terminal 1 — API**

```bash
cd src/EmployeeSeriesManagement.Api
dotnet run
```

- API URL: **https://localhost:7280** (also **http://localhost:5280**).
- Migrations + demo seed run automatically against the SQL Server container on first start.
- Sends OTLP to `http://localhost:4317` (configured in [appsettings.Development.json](src/EmployeeSeriesManagement.Api/appsettings.Development.json)); if the collector isn't running you get harmless warnings in the API logs, otherwise traces flow into Jaeger.

**Terminal 2 — Blazor UI**

```bash
cd src/EmployeeSeriesManagement.Web
dotnet run
```

- UI URL: **https://localhost:7231**.
- It calls the native API on `https://localhost:7280` (configured in [wwwroot/appsettings.json](src/EmployeeSeriesManagement.Web/wwwroot/appsettings.json)).
- Jaeger UI (if running): **http://localhost:16686**.

> **HTTPS dev certificate**: if it's your first time running an ASP.NET Core app, run `dotnet dev-certs https --trust` once.

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

## Observability — OpenTelemetry → Jaeger

The API emits OpenTelemetry traces with three layers of spans per request:

1. **ASP.NET Core** auto-instrumented incoming HTTP span (controller, route, status).
2. **Application** custom span `EmployeeService.<Operation>` with business tags (`app.employee.external_id`, `app.series.code`, `app.work_city`, `app.period.start/end`, `app.result.count`, `app.outcome`).
3. **Infrastructure** custom span `EmployeeRepository.<Operation>` + **SqlClient** span with the executed SQL command.

Custom `ActivitySource`s: `EmployeeSeriesManagement.Application`, `EmployeeSeriesManagement.Infrastructure` ([ApplicationTelemetry.cs](src/EmployeeSeriesManagement.Application/Diagnostics/ApplicationTelemetry.cs), [InfrastructureTelemetry.cs](src/EmployeeSeriesManagement.Infrastructure/Diagnostics/InfrastructureTelemetry.cs)). Pipeline wired in [Program.cs](src/EmployeeSeriesManagement.Api/Program.cs).

### Visualizing traces

API → OTLP gRPC → `otel-collector` (batch + memory limiter) → Jaeger (UI on `:16686`) and `debug` exporter (stdout).

Open **http://localhost:16686**, pick `EmployeeSeriesManagement.Api` from the *Service* dropdown, click *Find Traces*. Click any trace to see the waterfall with the `HTTP → Service → Repository → SQL` hierarchy and all `app.*` tags.

The collector also writes span counts to stdout via the `debug` exporter — useful to verify the pipeline without opening the UI:

```bash
docker compose logs -f otel-collector
```

### Swapping the backend

The collector layer means you can switch to a different trace backend without touching the API. Edit [docker/otel-collector/collector-config.yaml](docker/otel-collector/collector-config.yaml) and replace the `otlp/jaeger` exporter with any of `otlphttp` / `zipkin` / `prometheus` / vendor-specific exporters. Same OTLP receiver, different downstream.

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
