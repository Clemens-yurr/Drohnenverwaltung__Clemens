using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task CreateLoan_WithoutAccessories_CreatesLoanWithoutItems()
    {
        var droneId = await CreateDroneAsync("TELLO-09", "Drone without accessories");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");

        var response = await _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var loanId = json.RootElement.GetProperty("id").GetInt32();

        Assert.Empty(GetLoanItems(loanId));
    }

    [Fact]
    public async Task CreateLoan_WithBatteryAndPropellerQuantities_RecordsItems()
    {
        var droneId = await CreateDroneAsync("TELLO-10", "Drone with accessories");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");

        var response = await _client.PostAsJsonAsync("/api/loans", new
        {
            droneId,
            studentGroupId = groupId,
            items = new[]
            {
                new { type = "Battery", quantity = 2 },
                new { type = "Propeller", quantity = 4 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var loanId = json.RootElement.GetProperty("id").GetInt32();

        var items = GetLoanItems(loanId);

        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.Type == LoanItemType.Battery && item.Quantity == 2);
        Assert.Contains(items, item => item.Type == LoanItemType.Propeller && item.Quantity == 4);
    }

    [Fact]
    public async Task CreateLoan_WithZeroAccessoryQuantity_RecordsItem()
    {
        var droneId = await CreateDroneAsync("TELLO-11", "Drone without spare batteries");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");

        var response = await _client.PostAsJsonAsync("/api/loans", new
        {
            droneId,
            studentGroupId = groupId,
            items = new[] { new { type = "Battery", quantity = 0 } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var loanId = json.RootElement.GetProperty("id").GetInt32();

        var item = Assert.Single(GetLoanItems(loanId));
        Assert.Equal(LoanItemType.Battery, item.Type);
        Assert.Equal(0, item.Quantity);
    }

    [Fact]
    public async Task CreateLoan_WithNegativeQuantity_ReturnsValidationProblemAndCreatesNoLoan()
    {
        var droneId = await CreateDroneAsync("TELLO-12", "Drone with a negative quantity");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");
        var before = CountLoans();

        var response = await _client.PostAsJsonAsync("/api/loans", new
        {
            droneId,
            studentGroupId = groupId,
            items = new[] { new { type = "Propeller", quantity = -1 } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(before, CountLoans());
    }

    [Fact]
    public async Task CreateLoan_WithDuplicateAccessoryType_ReturnsValidationProblemAndCreatesNoLoan()
    {
        var droneId = await CreateDroneAsync("TELLO-13", "Drone with duplicate accessory types");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");
        var before = CountLoans();

        var response = await _client.PostAsJsonAsync("/api/loans", new
        {
            droneId,
            studentGroupId = groupId,
            items = new[]
            {
                new { type = "Battery", quantity = 1 },
                new { type = "Battery", quantity = 2 }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(before, CountLoans());
    }

    [Fact]
    public async Task CreateLoan_ForDroneWithActiveLoan_ReturnsConflictAndKeepsSingleActiveLoan()
    {
        var droneId = await CreateDroneAsync("TELLO-20", "Drone with an active loan");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");

        var first = await _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        Assert.Equal(409, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Drone not available", json.RootElement.GetProperty("title").GetString());

        var stored = Assert.Single(GetLoans(droneId));
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task CreateLoan_FromConcurrentRequests_CreatesExactlyOneActiveLoan()
    {
        var droneId = await CreateDroneAsync("TELLO-21", "Drone for concurrent loan requests");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");

        var responses = await Task.WhenAll(
            _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId }),
            _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId }));

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));

        var stored = Assert.Single(GetLoans(droneId));
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task Database_RejectsSecondActiveLoanForSameDrone()
    {
        var droneId = await CreateDroneAsync("TELLO-22", "Drone with a database constraint");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        dbContext.Loans.Add(new Loan
        {
            DroneId = droneId,
            StudentGroupId = groupId,
            LoanedOn = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        await dbContext.SaveChangesAsync();

        dbContext.Loans.Add(new Loan
        {
            DroneId = droneId,
            StudentGroupId = groupId,
            LoanedOn = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ReturnLoan_ForActiveLoan_RecordsTodaysDateAndFreesDrone()
    {
        var droneId = await CreateDroneAsync("TELLO-30", "Drone to be returned");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");
        var loanId = await CreateLoanAsync(droneId, groupId);

        var response = await _client.PostAsync($"/api/loans/{loanId}/return", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var returned = json.RootElement;

        Assert.Equal(loanId, returned.GetProperty("id").GetInt32());
        Assert.Equal(
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            returned.GetProperty("returnedOn").GetString());

        var stored = Assert.Single(GetLoans(droneId));
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), stored.ReturnedOn);
        Assert.False(stored.IsActive);

        Assert.Equal("Available", await GetDroneStatusAsync(droneId));
    }

    [Fact]
    public async Task ReturnLoan_ForReturnedLoan_ReturnsConflictAndKeepsReturnDate()
    {
        var droneId = await CreateDroneAsync("TELLO-31", "Drone returned twice");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");
        var loanId = await CreateLoanAsync(droneId, groupId);

        var first = await _client.PostAsync($"/api/loans/{loanId}/return", null);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var recordedReturnDate = Assert.Single(GetLoans(droneId)).ReturnedOn;

        var second = await _client.PostAsync($"/api/loans/{loanId}/return", null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        Assert.Equal(409, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Loan already returned", json.RootElement.GetProperty("title").GetString());

        Assert.Equal(recordedReturnDate, Assert.Single(GetLoans(droneId)).ReturnedOn);
    }

    [Fact]
    public async Task ReturnLoan_DoesNotOverwriteOriginalReturnDate()
    {
        var droneId = await CreateDroneAsync("TELLO-32", "Drone with an earlier return date");
        var groupId = SeedStudentGroup($"Group {Guid.NewGuid():N}");
        var loanId = await CreateLoanAsync(droneId, groupId);
        var originalReturnDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3);
        SetReturnDate(loanId, originalReturnDate);

        var response = await _client.PostAsync($"/api/loans/{loanId}/return", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var stored = Assert.Single(GetLoans(droneId));
        Assert.Equal(originalReturnDate, stored.ReturnedOn);
        Assert.False(stored.IsActive);
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

    private List<LoanItem> GetLoanItems(int loanId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        return dbContext.LoanItems
            .Where(item => item.LoanId == loanId)
            .OrderBy(item => item.Id)
            .ToList();
    }

    private async Task<int> CreateLoanAsync(int droneId, int groupId)
    {
        var response = await _client.PostAsJsonAsync("/api/loans", new { droneId, studentGroupId = groupId });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<string?> GetDroneStatusAsync(int droneId)
    {
        var response = await _client.GetAsync("/api/drones");
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.EnumerateArray()
            .Single(drone => drone.GetProperty("id").GetInt32() == droneId)
            .GetProperty("status")
            .GetString();
    }

    private void SetReturnDate(int loanId, DateOnly returnedOn)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        var loan = dbContext.Loans.Single(existing => existing.Id == loanId);
        loan.ReturnedOn = returnedOn;
        dbContext.SaveChanges();
    }

    private List<Loan> GetLoans(int droneId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        return dbContext.Loans
            .Where(loan => loan.DroneId == droneId)
            .OrderBy(loan => loan.Id)
            .ToList();
    }

    private int CountLoans()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();

        return dbContext.Loans.Count();
    }
}
