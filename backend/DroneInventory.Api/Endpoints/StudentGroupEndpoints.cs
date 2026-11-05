using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using DroneInventory.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DroneInventory.Api.Endpoints;

public static class StudentGroupEndpoints
{
    public static IEndpointRouteBuilder MapStudentGroupEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var studentGroups = endpoints.MapGroup("/api/student-groups");

        studentGroups.MapGet("", async (DroneInventoryDbContext dbContext) =>
        {
            var groups = await dbContext.StudentGroups
                .OrderBy(group => group.Id)
                .Select(group => new StudentGroupDto(group.Id, group.Name))
                .ToListAsync();

            return Results.Ok(groups);
        })
        .WithName("GetStudentGroups");

        studentGroups.MapPost("", async (CreateStudentGroupRequest request, DroneInventoryDbContext dbContext) =>
        {
            var errors = Validate(request);

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var group = new StudentGroup { Name = request.Name };

            dbContext.StudentGroups.Add(group);
            await dbContext.SaveChangesAsync();

            return Results.Created("/api/student-groups", new StudentGroupDto(group.Id, group.Name));
        })
        .WithName("CreateStudentGroup");

        return endpoints;
    }

    /// <summary>
    /// A student group needs a name. Invalid input is reported as problem details.
    /// </summary>
    private static Dictionary<string, string[]> Validate(CreateStudentGroupRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(CreateStudentGroupRequest.Name)] = ["A student group name is required."];
        }

        return errors;
    }
}
