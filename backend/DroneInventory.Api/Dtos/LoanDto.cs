using DroneInventory.Api.Domain;

namespace DroneInventory.Api.Dtos;

/// <summary>
/// A loan as returned by the API. The return date is null while the loan is active.
/// </summary>
public sealed record LoanDto(
    int Id,
    int DroneId,
    string DroneName,
    int StudentGroupId,
    string StudentGroupName,
    DateOnly LoanedOn,
    DateOnly? ReturnedOn);

/// <summary>
/// The payload for creating a loan. The loan date is set by the server, so it is not part of it.
/// Accessories are optional and are tracked only as quantities per type.
/// </summary>
public sealed record CreateLoanRequest(
    int DroneId,
    int StudentGroupId,
    IReadOnlyList<CreateLoanItemRequest>? Items = null);

/// <summary>
/// An accessory position of a loan: a battery or propeller quantity of at least 0.
/// </summary>
public sealed record CreateLoanItemRequest(LoanItemType Type, int Quantity);
