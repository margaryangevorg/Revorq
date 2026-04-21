namespace Revorq.API.Services.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(int buildingId, IFormFile file);
    Task DeleteAsync(string fileUrl);
}
