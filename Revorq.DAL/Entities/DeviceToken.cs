namespace Revorq.DAL.Entities;

public class DeviceToken : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
