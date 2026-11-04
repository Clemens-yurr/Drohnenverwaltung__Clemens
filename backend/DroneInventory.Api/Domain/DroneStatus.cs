namespace DroneInventory.Api.Domain;

/// <summary>
/// Availability of a drone. It is derived from active loans and never stored or edited directly.
/// </summary>
public enum DroneStatus
{
    Available,
    Loaned
}
