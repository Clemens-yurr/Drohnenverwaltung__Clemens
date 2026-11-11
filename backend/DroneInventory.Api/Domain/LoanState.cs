namespace DroneInventory.Api.Domain;

/// <summary>
/// State of a loan. It is derived from the return date and never stored or edited directly.
/// </summary>
public enum LoanState
{
    Active,
    Returned
}
