namespace Revorq.DAL.Entities;

public class MaintenanceOrderHistory
{
    public long OrderId { get; set; }
    public MaintenanceOrder Order { get; set; } = null!;

    public List<EngineerAssignment> Assignments { get; set; } = [];
}
