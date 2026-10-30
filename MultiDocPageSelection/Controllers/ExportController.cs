using Microsoft.AspNetCore.Mvc;
using MultiDocPageSelection.Models;
using MultiDocPageSelection.Services;

namespace MultiDocPageSelection.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly GdPictureService _gdPictureService;

    public ExportController(GdPictureService gdPictureService)
    {
        _gdPictureService = gdPictureService;
    }

    [HttpPost]
    public IActionResult Export([FromBody] ExportRequest request)
    {
        if (request.Pages.Count == 0)
            return BadRequest("No pages selected.");

        byte[]? result;
        string contentType;
        string fileName;

        if (request.Format.ToLowerInvariant() == "tiff")
        {
            result = _gdPictureService.ExportAsTiff(request.Pages);
            contentType = "image/tiff";
            fileName = "export.tiff";
        }
        else
        {
            result = _gdPictureService.ExportAsPdf(request.Pages);
            contentType = "application/pdf";
            fileName = "export.pdf";
        }

        if (result == null)
            return StatusCode(500, "Failed to generate export.");

        return File(result, contentType, fileName);
    }
}
