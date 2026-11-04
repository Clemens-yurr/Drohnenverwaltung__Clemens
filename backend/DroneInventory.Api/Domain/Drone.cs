namespace DroneInventory.Api.Domain;

public class Drone
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public ICollection<Loan> Loans { get; } = new List<Loan>();
}
