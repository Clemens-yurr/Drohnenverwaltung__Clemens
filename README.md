# Drohnenverwaltung__Clemens
SYP AI Projekt Drohnenverwaltung

## Backend (DroneInventory.Api)

ASP.NET Core 10 minimal API with EF Core and SQLite. The project lives in
`backend/DroneInventory.Api`, the integration tests in `backend/DroneInventory.Api.Tests`.

### Prerequisites

- **.NET 10 SDK** (tested with 10.0.402) - `dotnet --version`
- **No separate database server or SQLite installation.** The `Microsoft.EntityFrameworkCore.Sqlite`
  package bundles the native SQLite library, so the database is a plain file.
- Optional: the `sqlite3` command-line shell if you want to inspect `drone-lending.db` by hand.

### Restore tools and packages

The repository pins the EF Core CLI as a local tool in `.config/dotnet-tools.json`
(`dotnet-ef` 10.0.14). Run both commands from the repository root:

```bash
dotnet tool restore
dotnet restore backend/DroneInventory.Api
```

`dotnet tool restore` installs the pinned `dotnet-ef` version for this repository only; no global
tool install is needed. Verify it with `dotnet ef --version`.

### Database and migrations

The connection string is `ConnectionStrings:DroneLending` in
`backend/DroneInventory.Api/appsettings.json` and defaults to `Data Source=drone-lending.db`.
The file is created next to the running app the first time the API starts, so a fresh checkout
needs no manual database step.

Pending migrations are applied automatically on startup (`Database.Migrate()` in `Program.cs`).
To apply them without starting the API:

```bash
dotnet ef database update --project backend/DroneInventory.Api
```

When the model changes, add a migration and commit it together with the model change:

```bash
dotnet ef migrations add <MigrationName> --project backend/DroneInventory.Api
```

The schema covers the `Drones`, `StudentGroups`, `Loans` and `LoanItems` tables. Delete
`drone-lending.db` to start over from an empty database; all data is local and disposable.

### Run the API

```bash
cd backend/DroneInventory.Api
dotnet run
```

The `http` launch profile serves the API on <http://localhost:5214>:

- `GET http://localhost:5214/api/drones` - list drones
- `http://localhost:5214/openapi/v1.json` - OpenAPI document (Development only)

Use `dotnet run --launch-profile https` for the HTTPS profile on <https://localhost:7265>.
`DroneInventory.Api.http` contains ready-to-send requests for an HTTP client such as
Visual Studio Code's REST Client or JetBrains HTTP Client.
