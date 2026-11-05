using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DroneInventory.Api.Tests;

public sealed class StudentGroupEndpointsTests : IClassFixture<DroneInventoryApiFactory>
{
    private readonly HttpClient _client;

    public StudentGroupEndpointsTests(DroneInventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateStudentGroup_StoresGroupAndReturnsIt()
    {
        var response = await _client.PostAsJsonAsync("/api/student-groups", new { name = "Group 3" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var group = json.RootElement;

        Assert.True(group.GetProperty("id").GetInt32() > 0);
        Assert.Equal("Group 3", group.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetStudentGroups_ReturnsCreatedGroupsForLoanSelection()
    {
        await CreateGroupAsync("Group 10A");
        await CreateGroupAsync("Group 10B");

        var groups = await GetGroupsAsync();

        Assert.Contains(groups, listed => listed.GetProperty("name").GetString() == "Group 10A");
        Assert.Contains(groups, listed => listed.GetProperty("name").GetString() == "Group 10B");
        Assert.All(groups, listed => Assert.True(listed.GetProperty("id").GetInt32() > 0));
    }

    [Fact]
    public async Task CreateStudentGroup_WithoutName_ReturnsValidationProblemAndStoresNothing()
    {
        var before = (await GetGroupsAsync()).Count;

        var missingName = await _client.PostAsJsonAsync("/api/student-groups", new { });
        var blankName = await _client.PostAsJsonAsync("/api/student-groups", new { name = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, missingName.StatusCode);
        Assert.Equal("application/problem+json", missingName.Content.Headers.ContentType?.MediaType);

        using var missingJson = JsonDocument.Parse(await missingName.Content.ReadAsStringAsync());
        Assert.Equal(400, missingJson.RootElement.GetProperty("status").GetInt32());
        Assert.True(missingJson.RootElement.GetProperty("errors").TryGetProperty("Name", out _));

        Assert.Equal(HttpStatusCode.BadRequest, blankName.StatusCode);

        var groups = await GetGroupsAsync();
        Assert.Equal(before, groups.Count);
        Assert.DoesNotContain(groups, listed => listed.GetProperty("name").GetString() == "   ");
    }

    private async Task CreateGroupAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/student-groups", new { name });
        response.EnsureSuccessStatusCode();
    }

    private async Task<List<JsonElement>> GetGroupsAsync()
    {
        var response = await _client.GetAsync("/api/student-groups");
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.EnumerateArray().Select(group => group.Clone()).ToList();
    }
}
