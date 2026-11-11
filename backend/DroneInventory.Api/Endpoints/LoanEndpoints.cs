using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using DroneInventory.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DroneInventory.Api.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder MapLoanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var loans = endpoints.MapGroup("/api/loans");

        loans.MapPost("", async (CreateLoanRequest request, DroneInventoryDbContext dbContext) =>
        {
            var errors = Validate(request);

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var drone = await dbContext.Drones.FindAsync(request.DroneId);

            if (drone is null)
            {
                return Results.Problem(
                    title: "Drone not found",
                    detail: $"No drone with id {request.DroneId} exists.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            var studentGroup = await dbContext.StudentGroups.FindAsync(request.StudentGroupId);

            if (studentGroup is null)
            {
                return Results.Problem(
                    title: "Student group not found",
                    detail: $"No student group with id {request.StudentGroupId} exists.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var loan = new Loan
            {
                DroneId = drone.Id,
                StudentGroupId = studentGroup.Id,
                LoanedOn = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            foreach (var item in request.Items ?? [])
            {
                loan.Items.Add(new LoanItem { Type = item.Type, Quantity = item.Quantity });
            }

            dbContext.Loans.Add(loan);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new LoanDto(
                loan.Id,
                drone.Id,
                drone.Name,
                studentGroup.Id,
                studentGroup.Name,
                loan.LoanedOn,
                loan.ReturnedOn);

            return Results.Created($"/api/loans/{loan.Id}", response);
        })
        .WithName("CreateLoan");

        return endpoints;
    }

    /// <summary>
    /// Accessories are optional, but a quantity must be a nonnegative integer and each accessory
    /// type may appear at most once per loan. Invalid input is reported as problem details.
    /// </summary>
    private static Dictionary<string, string[]> Validate(CreateLoanRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var items = request.Items ?? [];

        foreach (var duplicateType in items.GroupBy(item => item.Type).Where(group => group.Count() > 1))
        {
            errors[$"Items.{duplicateType.Key}"] =
                [$"{duplicateType.Key} may only be specified once per loan."];
        }

        for (var index = 0; index < items.Count; index++)
        {
            if (items[index].Quantity < 0)
            {
                errors[$"Items[{index}].Quantity"] =
                    ["An accessory quantity must be greater than or equal to 0."];
            }
        }

        return errors;
    }
}
