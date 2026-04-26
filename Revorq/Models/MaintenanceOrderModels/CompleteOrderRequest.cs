using System.Text.Json.Serialization;
using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class CreateReportRequest
{
    [JsonPropertyName("jobStartedDate")]
    public DateTime? JobStartedDate { get; set; }
    [JsonPropertyName("completedDate")]
    public DateTime? CompletedDate { get; set; }
    [JsonPropertyName("issueDetected")]
    public bool IssueDetected { get; set; }
    [JsonPropertyName("visualCheckDone")]
    public bool VisualCheckDone { get; set; }
    [JsonPropertyName("adjustmentDone")]
    public bool AdjustmentDone { get; set; }
    [JsonPropertyName("cleaningDone")]
    public bool CleaningDone { get; set; }
    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; set; }
    [JsonPropertyName("images")]
    public List<IFormFile>? Images { get; set; }
    [JsonPropertyName("status")]
    public OrderStatus? Status { get; set; }
}
