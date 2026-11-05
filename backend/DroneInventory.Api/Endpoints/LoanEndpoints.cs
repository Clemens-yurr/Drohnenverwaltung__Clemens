using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using DroneInventory.Api.Dtos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DroneInventory.Api.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder MapLoanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var loans = endpoints.MapGroup("/api/loans");

        loans.MapGet("", async (string? state, DroneInventoryDbContext dbContext) =>
        {
            var filter = state?.Trim().ToLowerInvariant();

            if (filter is not null and not ("" or "active" or "returned"))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["state"] = ["The loan state must be 'active' or 'returned'."]
                });
            }

            var query = dbContext.Loans.AsNoTracking();

            query = filter switch
            {
                "active" => query.Where(loan => loan.ReturnedOn == null),
                "returned" => query.Where(loan => loan.ReturnedOn != null),
                _ => query
            };

            // Accessory quantities are read from the loan item rows; a missing type counts as zero.
            var response = await query
                .OrderBy(loan => loan.Id)
                .Select(loan => new LoanListItemDto(
                    loan.Id,
                    loan.DroneId,
                    loan.Drone.Name,
                    loan.StudentGroupId,
                    loan.StudentGroup.Name,
                    loan.Items
                        .Where(item => item.Type == LoanItemType.Battery)
                        .Sum(item => (int?)item.Quantity) ?? 0,
                    loan.Items
                        .Where(item => item.Type == LoanItemType.Propeller)
                        .Sum(item => (int?)item.Quantity) ?? 0,
                    loan.LoanedOn,
                    loan.ReturnedOn == null ? LoanState.Active : LoanState.Returned,
                    loan.ReturnedOn))
                .ToListAsync();

            return Results.Ok(response);
        })
        .WithName("GetLoans");

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

            // A drone has at most one active loan. This readable check is backed up by the filtered
            // unique index on Loans.DroneId, which also catches requests that race past it.
            var droneIsLoaned = await dbContext.Loans
                .AnyAsync(loan => loan.DroneId == drone.Id && loan.ReturnedOn == null);

            if (droneIsLoaned)
            {
                return DroneNotAvailable(drone.Id);
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

            try
            {
                dbContext.Loans.Add(loan);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException exception) when (IsActiveLoanConflict(exception))
            {
                return DroneNotAvailable(drone.Id);
            }

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

        loans.MapPost("/{id:int}/return", async (int id, DroneInventoryDbContext dbContext) =>
        {
            var loan = await dbContext.Loans
                .Include(existing => existing.Drone)
                .Include(existing => existing.StudentGroup)
                .SingleOrDefaultAsync(existing => existing.Id == id);

            if (loan is null)
            {
                return Results.Problem(
                    title: "Loan not found",
                    detail: $"No loan with id {id} exists.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            if (!loan.IsActive)
            {
                return Results.Problem(
                    title: "Loan already returned",
                    detail: $"Loan {loan.Id} was already returned on {loan.ReturnedOn:yyyy-MM-dd}.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            // The return date is set by the server; clients cannot backdate it.
            loan.ReturnedOn = DateOnly.FromDateTime(DateTime.UtcNow);
            await dbContext.SaveChangesAsync();

            var response = new LoanDto(
                loan.Id,
                loan.DroneId,
                loan.Drone.Name,
                loan.StudentGroupId,
                loan.StudentGroup.Name,
                loan.LoanedOn,
                loan.ReturnedOn);

            return Results.Ok(response);
        })
        .WithName("ReturnLoan");

        return endpoints;
    }

    private static IResult DroneNotAvailable(int droneId) =>
        Results.Problem(
            title: "Drone not available",
            detail: $"Drone {droneId} already has an active loan.",
            statusCode: StatusCodes.Status409Conflict);

    /// <summary>
    /// SQLite reports a violation of the filtered unique index as a constraint error naming the
    /// indexed column, which is how a second active loan from a concurrent request is recognized.
    /// </summary>
    private static bool IsActiveLoanConflict(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 } sqliteException
        && sqliteException.Message.Contains("Loans.DroneId", StringComparison.Ordinal);

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
