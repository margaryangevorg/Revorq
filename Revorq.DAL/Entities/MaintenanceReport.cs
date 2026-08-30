namespace Revorq.DAL.Entities;

public class MaintenanceReport : IAuditable
{
    public int OrderId { get; set; }
    public MaintenanceOrder MaintenanceOrder { get; set; } = null!;

    public DateTime? JobStartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public bool IssueDetected { get; set; }
    public bool VisualCheckDone { get; set; }
    public bool AdjustmentDone { get; set; }
    public bool CleaningDone { get; set; }
    public bool IsPartChange { get; set; }

    public string? Notes { get; set; }
    public List<string> ImageUrls { get; set; } = [];

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
