using Microsoft.AspNetCore.Mvc;
using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class CreateReportRequest
{
    [FromForm(Name = "jobStartedDate")]
    public DateTime? JobStartedDate { get; set; }
    [FromForm(Name = "completedDate")]
    public DateTime? CompletedDate { get; set; }
    [FromForm(Name = "issueDetected")]
    public bool IssueDetected { get; set; }
    [FromForm(Name = "visualCheckDone")]
    public bool VisualCheckDone { get; set; }
    [FromForm(Name = "adjustmentDone")]
    public bool AdjustmentDone { get; set; }
    [FromForm(Name = "cleaningDone")]
    public bool CleaningDone { get; set; }
    [FromForm(Name = "shortDescription")]
    public string? ShortDescription { get; set; }
    [FromForm(Name = "images")]
    public List<IFormFile>? Images { get; set; }
    [FromForm(Name = "status")]
    public OrderStatus? Status { get; set; }
}
