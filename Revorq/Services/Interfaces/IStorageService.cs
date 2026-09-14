namespace Revorq.API.Services.Interfaces;

public interface IStorageService
{
    Task<string> UploadBuildingFileAsync(int buildingId, IFormFile file);
    Task<string> UploadCompanyLogoAsync(int companyId, IFormFile file);
    Task<string> UploadMaintenanceOrderImageAsync(long orderId, IFormFile file);
    Task<string> UploadMaintenanceReportImageAsync(long orderId, IFormFile file);
    Task DeleteFileAsync(string fileUrl);
    Task<string> UploadDocumentAsync(IFormFile file);
    Task<IEnumerable<string>> GetDocumentsAsync();
}
