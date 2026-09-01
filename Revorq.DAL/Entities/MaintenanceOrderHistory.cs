namespace Revorq.DAL.Entities;

public class MaintenanceOrderHistory
{
    public int OrderId { get; set; }
    public MaintenanceOrder Order { get; set; } = null!;

    public List<EngineerAssignment> Assignments { get; set; } = [];
}
