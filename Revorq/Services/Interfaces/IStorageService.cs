namespace Revorq.API.Services.Interfaces;

public interface IStorageService
{
    Task<string> UploadBuildingFileAsync(int buildingId, IFormFile file);
    Task<string> UploadCompanyLogoAsync(int companyId, IFormFile file);
    Task<string> UploadMaintenanceReportImageAsync(int orderId, IFormFile file);
    Task DeleteFileAsync(string fileUrl);
}
