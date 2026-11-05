using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace DroneInventory.Api.Tests;

/// <summary>
/// Hosts the API with its own SQLite database file, so test classes do not share state with each
/// other or with a local development database.
/// </summary>
public sealed class DroneInventoryApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"drone-inventory-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DroneLending", $"Data Source={_databasePath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        SqliteConnection.ClearAllPools();

        try
        {
            File.Delete(_databasePath);
        }
        catch (IOException)
        {
            // A leftover temp database is harmless; the next test run uses a fresh file.
        }
    }
}
