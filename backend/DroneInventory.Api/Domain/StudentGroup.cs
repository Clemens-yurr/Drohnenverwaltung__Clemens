namespace DroneInventory.Api.Domain;

public class StudentGroup
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Loan> Loans { get; } = new List<Loan>();
}
