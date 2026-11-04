namespace DroneInventory.Api.Domain;

public class Loan
{
    public int Id { get; set; }

    public int DroneId { get; set; }

    public Drone Drone { get; set; } = null!;

    public int StudentGroupId { get; set; }

    public StudentGroup StudentGroup { get; set; } = null!;

    public DateOnly LoanedOn { get; set; }

    public DateOnly? ReturnedOn { get; set; }

    public ICollection<LoanItem> Items { get; } = new List<LoanItem>();

    public bool IsActive => ReturnedOn is null;
}
