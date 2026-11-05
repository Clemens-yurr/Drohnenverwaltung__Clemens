using DroneInventory.Api.Data;
using DroneInventory.Api.Domain;
using DroneInventory.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DroneInventory.Api.Endpoints;

public static class DroneEndpoints
{
    public static IEndpointRouteBuilder MapDroneEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var drones = endpoints.MapGroup("/api/drones");

        drones.MapGet("", async (DroneInventoryDbContext dbContext) =>
        {
            var drones = await dbContext.Drones
                .OrderBy(drone => drone.Id)
                .Select(drone => new DroneDto(
                    drone.Id,
                    drone.Name,
                    drone.Description,
                    dbContext.Loans.Any(loan => loan.DroneId == drone.Id && loan.ReturnedOn == null)
                        ? DroneStatus.Loaned
                        : DroneStatus.Available))
                .ToListAsync();

            return Results.Ok(drones);
        })
        .WithName("GetDrones");

        drones.MapPost("", async (CreateDroneRequest request, DroneInventoryDbContext dbContext) =>
        {
            var errors = Validate(request);

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var drone = new Drone
            {
                Name = request.Name,
                Description = request.Description
            };

            dbContext.Drones.Add(drone);
            await dbContext.SaveChangesAsync();

            var response = new DroneDto(drone.Id, drone.Name, drone.Description, DroneStatus.Available);

            return Results.Created("/api/drones", response);
        })
        .WithName("CreateDrone");

        return endpoints;
    }

    /// <summary>
    /// A drone needs a name and a description. Invalid input is reported as problem details.
    /// </summary>
    private static Dictionary<string, string[]> Validate(CreateDroneRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(CreateDroneRequest.Name)] = ["A drone name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors[nameof(CreateDroneRequest.Description)] = ["A drone description is required."];
        }

        return errors;
    }
}
