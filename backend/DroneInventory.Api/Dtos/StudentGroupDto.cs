namespace DroneInventory.Api.Dtos;

/// <summary>
/// A student group as returned by the API and offered for selection in the loan form.
/// </summary>
public sealed record StudentGroupDto(int Id, string Name);

/// <summary>
/// The payload for creating a student group.
/// </summary>
public sealed record CreateStudentGroupRequest(string Name);
