using DroneInventory.Api.Domain;

namespace DroneInventory.Api.Dtos;

/// <summary>
/// A drone as returned by the API, including its status derived from active loans.
/// </summary>
public sealed record DroneDto(int Id, string Name, string Description, DroneStatus Status);

/// <summary>
/// The payload for creating a drone. The status is not part of it because it is derived.
/// </summary>
public sealed record CreateDroneRequest(string Name, string Description);
