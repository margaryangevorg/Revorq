using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Revorq.API.Services.Interfaces;

namespace Revorq.API.Services.Implementations;

public class GoogleStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public GoogleStorageService(IConfiguration config)
    {
        _bucketName = config["GoogleCloud:BucketName"]!;

        var credentialJson = config["GoogleCloud:CredentialJson"];
        var credentialPath = config["GoogleCloud:CredentialPath"];

        GoogleCredential credential;
        if (!string.IsNullOrEmpty(credentialJson))
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialJson));
            credential = GoogleCredential.FromStream(stream);
        }
        else
        {
            credential = GoogleCredential.FromStream(File.OpenRead(credentialPath!));
        }

        _storageClient = StorageClient.Create(credential);
    }

    public Task<string> UploadBuildingFileAsync(int buildingId, IFormFile file)
    {
        var folder = file.ContentType.StartsWith("image/") ? "images" : "documents";
        var ext = Path.GetExtension(file.FileName);
        return UploadAsync($"buildings/{buildingId}/{folder}/{Guid.NewGuid()}{ext}", file);
    }

    public Task<string> UploadCompanyLogoAsync(int companyId, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        return UploadAsync($"companies/{companyId}/logo_{Guid.NewGuid()}{ext}", file);
    }

    public Task<string> UploadMaintenanceOrderImageAsync(int orderId, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        return UploadAsync($"maintenanceOrders/{orderId}/images/{Guid.NewGuid()}{ext}", file);
    }

    public Task<string> UploadMaintenanceReportImageAsync(int orderId, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        return UploadAsync($"maintenanceReports/{orderId}/images/{Guid.NewGuid()}{ext}", file);
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        var prefix = $"https://storage.googleapis.com/{_bucketName}/";
        var objectName = fileUrl[prefix.Length..];
        await _storageClient.DeleteObjectAsync(_bucketName, objectName);
    }

    private async Task<string> UploadAsync(string objectName, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        await _storageClient.UploadObjectAsync(_bucketName, objectName, file.ContentType, stream);
        return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
    }
}
