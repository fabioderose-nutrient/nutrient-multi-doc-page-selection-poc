using Microsoft.AspNetCore.Mvc;
using MultiDocPageSelection.Services;

namespace MultiDocPageSelection.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly GdPictureService _gdPictureService;

    public DocumentsController(GdPictureService gdPictureService)
    {
        _gdPictureService = gdPictureService;
    }

    [HttpGet]
    public IActionResult GetDocuments()
    {
        var documents = _gdPictureService.GetDocuments();
        return Ok(documents);
    }

    [HttpGet("{documentId}/pages/{pageNumber}/thumbnail")]
    public IActionResult GetThumbnail(string documentId, int pageNumber, [FromQuery] int width = 150)
    {
        var thumbnail = _gdPictureService.GetThumbnail(documentId, pageNumber, width);
        if (thumbnail == null)
            return NotFound();

        return File(thumbnail, "image/png");
    }
}
