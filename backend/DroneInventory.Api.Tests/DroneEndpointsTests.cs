using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DroneInventory.Api.Tests;

public sealed class DroneEndpointsTests : IClassFixture<DroneInventoryApiFactory>
{
    private readonly DroneInventoryApiFactory _factory;
    private readonly HttpClient _client;

    public DroneEndpointsTests(DroneInventoryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDrone_StoresDroneAndReturnsIt()
    {
        var response = await _client.PostAsJsonAsync("/api/drones", new
        {
            name = "Copter One",
            description = "Training drone"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var drone = json.RootElement;

        Assert.True(drone.GetProperty("id").GetInt32() > 0);
        Assert.Equal("Copter One", drone.GetProperty("name").GetString());
        Assert.Equal("Training drone", drone.GetProperty("description").GetString());
        Assert.Equal("Available", drone.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CreateDrone_WithoutNameAndDescription_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/drones", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var problem = json.RootElement;

        Assert.Equal(400, problem.GetProperty("status").GetInt32());

        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Description", out _));
    }

    [Fact]
    public async Task CreateDrone_WithBlankDescription_ReturnsValidationProblemAndStoresNothing()
    {
        var before = (await GetDronesAsync()).Count;

        var response = await _client.PostAsJsonAsync("/api/drones", new
        {
            name = "Copter Without Description",
            description = "   "
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("Description", out _));

        var drones = await GetDronesAsync();
        Assert.Equal(before, drones.Count);
        Assert.DoesNotContain(drones, listed =>
            listed.GetProperty("name").GetString() == "Copter Without Description");
    }

    [Fact]
    public async Task GetDrones_ReturnsNameDescriptionAndAvailableStatus()
    {
        await CreateDroneAsync("Copter Two", "Second training drone");

        var drones = await GetDronesAsync();

        var drone = Assert.Single(drones, listed => listed.GetProperty("name").GetString() == "Copter Two");
        Assert.Equal("Second training drone", drone.GetProperty("description").GetString());
        Assert.Equal("Available", drone.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetDrones_ReportsDroneWithActiveLoanAsLoaned()
    {
        var droneId = await CreateDroneAsync("Copter Loaned", "Currently out");
        var availableId = await CreateDroneAsync("Copter Free", "Still in the shelf");
        var loanId = SeedActiveLoan(droneId);

        var drones = await GetDronesAsync();

        var loaned = Assert.Single(drones, listed => listed.GetProperty("id").GetInt32() == droneId);
        Assert.Equal("Loaned", loaned.GetProperty("status").GetString());

        var available = Assert.Single(drones, listed => listed.GetProperty("id").GetInt32() == availableId);
        Assert.Equal("Available", available.GetProperty("status").GetString());

        ReturnLoan(loanId);

        var afterReturn = Assert.Single(await GetDronesAsync(), listed => listed.GetProperty("id").GetInt32() == droneId);
        Assert.Equal("Available", afterReturn.GetProperty("status").GetString());
    }

    private async Task<int> CreateDroneAsync(string name, string description)
    {
        var response = await _client.PostAsJsonAsync("/api/drones", new { name, description });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<List<JsonElement>> GetDronesAsync()
    {
        var response = await _client.GetAsync("/api/drones");
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.EnumerateArray().Select(drone => drone.Clone()).ToList();
    }

    /// <summary>
    /// Loans are created by the loan API (a later task), so the active loan is seeded directly.
    /// </summary>
    private int SeedActiveLoan(int droneId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        var group = new StudentGroup { Name = $"Group {Guid.NewGuid():N}" };
        var loan = new Loan
        {
            DroneId = droneId,
            StudentGroup = group,
            LoanedOn = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        dbContext.Loans.Add(loan);
        dbContext.SaveChanges();

        return loan.Id;
    }

    private void ReturnLoan(int loanId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        var loan = dbContext.Loans.Single(existing => existing.Id == loanId);
        loan.ReturnedOn = DateOnly.FromDateTime(DateTime.UtcNow);
        dbContext.SaveChanges();
    }
}
