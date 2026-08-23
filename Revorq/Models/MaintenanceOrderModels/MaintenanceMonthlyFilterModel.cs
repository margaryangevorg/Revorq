using Revorq.DAL.Enums;

namespace Revorq.Models.MaintenanceOrderModels;

public class MaintenanceMonthlyFilterModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public bool? IsUnassigned { get; set; }
    public bool? IsScheduled { get; set; }
    public List<OrderStatus> Statuses { get; set; }
}
