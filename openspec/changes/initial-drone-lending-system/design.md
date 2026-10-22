# Design

## Context

See `proposal.md` - Why. The repository currently contains empty scaffolds: `backend/DroneInventory.Api`
is a .NET 10 minimal-API template with only `Program.cs`, `Microsoft.AspNetCore.OpenApi` and no
persistence, and `frontend/drone-inventory-ui` is the default Angular 22 standalone app with a
placeholder page and an empty route table. There is no solution file, no test project and no
`dotnet-ef` global tool; Node 26 and npm 11 are installed and `node_modules` is present.

Hard constraints from the project context: C# / ASP.NET Core 10 / EF Core / SQLite on the backend,
Angular + TypeScript on the frontend, and the domain rules (one active loan per drone, only
available drones can be loaned, returning frees the drone, accessory quantities never negative).
Requirements live in `specs/drone-management`, `specs/student-group-management` and
`specs/drone-lending`.

## Goals / Non-Goals

**Goals:**

- One EF Core model with `Drone`, `StudentGroup`, `Loan`, `LoanItem` that makes the domain rules
  impossible to violate through concurrent or careless API use.
- A JSON API whose resources and error responses are predictable enough for the Angular client to
  render lists, forms and history without client-side business rules.
- A single shared definition of "active loan" and "drone availability", derived rather than stored
  twice.

**Non-Goals:**

- Authentication, accounts, roles (explicitly out of scope per the request).
- Tracking individual batteries/propellers or any accessory stock ledger.
- Reservations, loan extensions, multi-drone loans, damage/loss reporting.
- Deployment, containerization, or a production hosting story.
- A shared TypeScript type generator - the client models are hand-written and small.

## Decisions

### Persistence: EF Core + SQLite, schema via migrations

`DroneInventoryDbContext` with `DbSet<Drone>`, `DbSet<StudentGroup>`, `DbSet<Loan>`,
`DbSet<LoanItem>`. Connection string `Data Source=drone-lending.db` in `appsettings.json`, overridable
for tests. To evolve the schema without losing loan history, use EF Core migrations
(`Microsoft.EntityFrameworkCore.Design` + a local `dotnet-ef` tool manifest) and apply them with
`Database.Migrate()` at startup instead of `EnsureCreated()`.

Alternatives: `EnsureCreated()` (faster first run, but any later property change requires dropping
the database - unacceptable once real loans exist); a document store/JSON files (weaker querying and
no relational integrity for loans).

### Availability and loan state are derived, not stored

A loan is active when `ReturnedOn IS NULL`; a drone is `OnLoan` when any active loan references it,
otherwise `Available`. The API projects `status`/`state` in queries, so no stored flag can drift out
of sync with the loan rows. `status` is never accepted as input.

Alternatives: store `Drone.Status` and flip it on loan/return (fast to write, but two sources of
truth and an easy way to end up with a loaned drone marked available); store `Loan.IsActive`
(redundant with the return date and needs its own consistency checks).

### Domain rules enforced in the database and in a loan service, not only in the endpoints

- Unique indexes on `Drone.InventoryNumber` and `StudentGroup.Name`.
- A partial unique index on `Loan.DroneId` filtered to `"ReturnedOn" IS NULL`, so "at most one
  active loan per drone" holds even if two requests race.
- `LoanService` performs the readable checks (drone exists and is available, group exists, quantities
  `>= 0`, not already returned, no loan history before delete) inside a transaction and returns a
  result object with a domain error code; endpoints translate codes to HTTP responses.

Alternatives: endpoint-level checks only (duplicates and double loans become possible under
concurrency); EF interceptors/domain events (more machinery than a single-user tool needs).

### Status codes and error shape

`404` for unknown ids, `400` (ValidationProblemDetails) for malformed or negative input, `409` for
rule conflicts (inventory number or group name already used, drone not available, loan already
returned, delete blocked by loan history). A unique-index violation that slips past the service check
is caught as `DbUpdateException` and mapped to the same `409` shape.

### Dates: server-set `DateOnly`, injectable clock

`LoanedOn` is set by the server on creation and `ReturnedOn` on return; clients cannot backdate.
The service takes `TimeProvider` from DI so integration tests can pin "today" without extra
packages. `DateOnly` maps to a SQLite `TEXT` date and serialises as `yyyy-MM-dd`.

### Accessories as `LoanItem` rows with an enum type

`LoanItem { LoanId, Type (Battery | Propeller), Quantity int }` with `Type` persisted as a string.
Zero quantities are allowed and simply mean "none of that accessory"; omitted accessories mean no
rows. There is at most one row per type per loan (validated by the service and a unique index on
`(LoanId, Type)`). No stock level exists to decrement, per the request.

Alternative: two integer columns `BatteryCount`/`PropellerCount` on `Loan` (less code, but a fixed
schema per accessory type and no natural place for the `Quantity >= 0` check pattern).

### API surface (minimal APIs, versionless `/api`)

- `GET/POST /api/drones` (with `?status=available|onloan` for the loan-form picker), `GET/PUT/DELETE /api/drones/{id}`
- `GET/POST /api/student-groups`, `PUT/DELETE /api/student-groups/{id}`
- `GET /api/loans?state=active|returned&droneId=&studentGroupId=` - also serves as loan history
- `GET /api/loans/{id}`
- `POST /api/loans` with `{ droneId, studentGroupId, items: [{ type, quantity }] }`
- `POST /api/loans/{id}/return`

DTOs are separate records from entities so navigation properties and ids never leak or become
bindable by accident. JSON uses the default camelCase contract; enum values are sent as strings
(`"Battery"`, `"OnLoan"`). The weather-forecast sample is deleted.

### Frontend: standalone components with signals and per-feature folders

- `src/app/core/` - hand-written API models plus `DroneApiService`, `StudentGroupApiService`,
  `LoanApiService` wrapping `HttpClient`.
- `src/app/features/drones/`, `.../student-groups/`, `.../loans/` - list/editor components for each
  area; one page for creating a loan (drone + group pickers, accessory quantities) and one for
  history with state/drone/group filters.
- Routes are lazy-loaded via `loadComponent`; `provideHttpClient()` is added to `app.config.ts`;
  `app.html` becomes a shell with navigation and `<router-outlet />`, replacing the placeholder.
- The dev server proxies `/api` to `http://localhost:5214` through `proxy.conf.json` referenced in
  `angular.json`, so no CORS configuration is needed in development and fetch URLs stay relative.

Alternatives: NgRx (state management the app does not need yet); generating a typed client from
OpenAPI (nice, but overkill for five resources and would add a build step).

### Verification

- A new `backend/DroneInventory.Api.Tests` xUnit project using `Microsoft.AspNetCore.Mvc.Testing`
  with a temp-file SQLite database per test class, covering every scenario in the three specs,
  including negative quantities, double loan, double return and blocked deletes. `public partial
  class Program` is added so `WebApplicationFactory<Program>` can host the app.
- Frontend: Angular's Vitest setup (`ng test`) for the API services with
  `provideHttpClientTesting` and for the accessory-quantity form validation; `ng build` for
  type/ template checking.

## Risks / Trade-offs

- **Partial unique index depends on SQLite's index filter syntax** → verify the generated migration
  contains `WHERE "ReturnedOn" IS NULL`, and keep the service-level check as the primary guard so a
  wrong index degrades to a 409 from the service rather than silent corruption.
- **Migrations require a local `dotnet-ef` tool and NuGet restore** → add a tool manifest to the
  repository (`dotnet new tool-manifest`, `dotnet tool restore`) so any machine can regenerate them;
  if restore is unavailable, fall back to `EnsureCreated()` for the first run and note it.
- **`DateOnly` + SQLite + EF Core provider quirks** → covered by the integration tests that assert
  loan/return dates, and kept to two columns.
- **Delete blocked by history** means a mistyped drone or group cannot be removed once used → state
  this in the UI error message; a soft-delete flag can be added later without changing the specs.
- **No auth means anyone reaching the API can lend or return** → accepted for this initial,
  explicitly single-user scope; the API stays unauthenticated behind the dev proxy.
- **Client-side duplication of enum strings** → kept small and centralised in one models file; the
  specs, not the client, remain the source of truth.

## Migration Plan

1. Add EF Core SQLite + Design packages, model, context and initial migration; apply at startup.
2. Replace the sample endpoint with the resources above, keeping `/openapi` for exploration.
3. Replace the Angular placeholder page with the shell and feature screens, proxying `/api`.
4. Rollback: revert the commit; the SQLite file is local and disposable, and no external system
   depends on the API yet.
