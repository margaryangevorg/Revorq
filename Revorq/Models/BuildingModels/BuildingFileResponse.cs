namespace Revorq.API.Models.BuildingModels;

public class BuildingFileResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
