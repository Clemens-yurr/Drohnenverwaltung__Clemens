using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DroneInventory.Api.Tests;

public sealed class LoanEndpointsTests : IClassFixture<DroneInventoryApiFactory>
{
    private readonly DroneInventoryApiFactory _factory;
    private readonly HttpClient _client;

    public LoanEndpointsTests(DroneInventoryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateLoan_ForExistingDroneAndGroup_CreatesActiveLoanWithCurrentDate()
    {
        var droneId = await CreateDroneAsync("TELLO-07", "Drone for lending");
        var groupName = $"Group {Guid.NewGuid():N}";
        var groupId = SeedStudentGroup(groupName);

        var response = await _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var loan = json.RootElement;
        var loanId = loan.GetProperty("id").GetInt32();

        Assert.True(loanId > 0);
        Assert.Equal(droneId, loan.GetProperty("droneId").GetInt32());
        Assert.Equal("TELLO-07", loan.GetProperty("droneName").GetString());
        Assert.Equal(groupId, loan.GetProperty("studentGroupId").GetInt32());
        Assert.Equal(groupName, loan.GetProperty("studentGroupName").GetString());
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), loan.GetProperty("loanedOn").GetString());
        Assert.Equal(JsonValueKind.Null, loan.GetProperty("returnedOn").ValueKind);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        var stored = dbContext.Loans.Single(existing => existing.Id == loanId);
        Assert.Equal(droneId, stored.DroneId);
        Assert.Equal(groupId, stored.StudentGroupId);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), stored.LoanedOn);
        Assert.Null(stored.ReturnedOn);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task CreateLoan_WithUnknownStudentGroup_ReturnsNotFoundAndCreatesNoLoan()
    {
        var droneId = await CreateDroneAsync("TELLO-08", "Drone without a group");
        var before = CountLoans();

        var response = await _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = 999_999 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, CountLoans());
    }

    [Fact]
    public async Task CreateLoan_WithUnknownDrone_ReturnsNotFoundAndCreatesNoLoan()
    {
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");
        var before = CountLoans();

        var response = await _client.PostAsJsonAsync("/api/loans", new { droneId = 999_999, studentGroupId = groupId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, CountLoans());
    }

    private async Task<int> CreateDroneAsync(string name, string description)
    {
        var response = await _client.PostAsJsonAsync("/api/drones", new { name, description });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetInt32();
    }

    /// <summary>
    /// Student groups are created by the student group API (a later task), so groups are seeded directly.
    /// </summary>
    private int SeedStudentGroup(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        var group = new StudentGroup { Name = name };

        dbContext.StudentGroups.Add(group);
        dbContext.SaveChanges();

        return group.Id;
    }

    private int CountLoans()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        return dbContext.Loans.Count();
    }
}
