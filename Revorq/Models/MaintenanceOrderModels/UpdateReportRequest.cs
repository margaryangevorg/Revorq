using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class UpdateReportRequest
{
    public DateTime? JobStartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IssueDetected { get; set; }
    public bool VisualCheckDone { get; set; }
    public bool AdjustmentDone { get; set; }
    public bool CleaningDone { get; set; }
    public bool IsPartChange { get; set; }
    public string? Notes { get; set; }
    public OrderStatus? Status { get; set; }
}
