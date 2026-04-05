namespace Revorq.DAL.Entities;

public class MaintenanceReport
{
    public int OrderId { get; set; }
    public MaintenanceOrder MaintenanceOrder { get; set; } = null!;

    public DateTime? JobStartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public bool IssueDetected { get; set; }
    public bool VisualCheckDone { get; set; }
    public bool AdjustmentDone { get; set; }
    public bool CleaningDone { get; set; }

    public string? ShortDescription { get; set; }
    public string? ImagePath { get; set; }
}
