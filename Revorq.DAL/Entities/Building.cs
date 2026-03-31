namespace Revorq.DAL.Entities;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }

    public ICollection<Elevator> Elevators { get; set; } = [];
}
