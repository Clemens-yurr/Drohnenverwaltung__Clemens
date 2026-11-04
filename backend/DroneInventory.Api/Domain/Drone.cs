namespace DroneInventory.Api.Domain;

/// <summary>
/// A drone that can be loaned to a student group. Its availability is derived from active
/// loans rather than stored, so the entity has no status field.
/// </summary>
public sealed class Drone
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
