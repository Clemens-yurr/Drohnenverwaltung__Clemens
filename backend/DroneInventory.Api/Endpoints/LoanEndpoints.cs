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
}
