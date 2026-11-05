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
/// </summary>
public sealed record CreateLoanRequest(int DroneId, int StudentGroupId);
