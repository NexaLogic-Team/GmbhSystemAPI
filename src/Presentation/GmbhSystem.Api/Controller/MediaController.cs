using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    // 1. POST: api/cms/media/upload (Image & Video Write to R2)
    [HttpPost("upload")]
    // [Authorize]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".avi", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Only images and videos are allowed." });
            }

            var fileName = await _mediaService.UploadFileAsync(file, cancellationToken);
            var fileUrl = $"{Request.Scheme}://{Request.Host}/api/cms/media/{fileName}";

            return Ok(new
            {
                message = "Media uploaded successfully to Cloudflare R2.",
                fileName = fileName,
                url = fileUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Upload failed", error = ex.Message });
        }
    }

    // 2. GET: api/cms/media/{fileName} (Image & Video Read from R2)
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Get(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var (stream, contentType) = await _mediaService.GetFileAsync(fileName, cancellationToken);
            return File(stream, contentType);
        }
        catch
        {
            return NotFound(new { message = "File not found in R2 storage." });
        }
    }
}