namespace Revorq.DAL.Entities;

public interface IAuditable
{
    DateTime CreatedDate { get; set; }
    DateTime UpdatedDate { get; set; }
}
