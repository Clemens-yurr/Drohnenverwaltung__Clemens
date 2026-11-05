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
}
