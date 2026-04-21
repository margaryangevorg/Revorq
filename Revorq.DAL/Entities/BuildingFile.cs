namespace Revorq.DAL.Entities;

public class BuildingFile
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public required string OriginalName { get; set; }
    public required string ContentType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;
}
