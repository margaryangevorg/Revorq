using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
public class DocumentsController : ControllerBase
{
    private readonly IStorageService _storageService;

    public DocumentsController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
    {
        if (files.Count == 0)
            return BadRequest("No files provided.");

        if (files.Any(f => !f.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)))
            return BadRequest("Only PDF files are allowed.");

        var uploadTasks = files.Select(f => _storageService.UploadDocumentAsync(f));
        var urls = await Task.WhenAll(uploadTasks);
        return Ok(urls);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var urls = await _storageService.GetDocumentsAsync();
        return Ok(urls);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] List<string> fileUrls)
    {
        if (fileUrls.Count == 0)
            return BadRequest("No file URLs provided.");

        await Task.WhenAll(fileUrls.Select(url => _storageService.DeleteFileAsync(url)));
        return Ok();
    }
}
