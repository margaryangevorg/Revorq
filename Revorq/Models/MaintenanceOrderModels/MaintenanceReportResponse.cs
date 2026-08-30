namespace Revorq.API.Models.MaintenanceOrderModels;

public class MaintenanceReportResponse
{
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
