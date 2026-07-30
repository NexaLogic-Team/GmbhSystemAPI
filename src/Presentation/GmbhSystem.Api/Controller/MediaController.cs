using GmbhSystem.Infrastructure.Services;
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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file selected." });

        try
        {
            using var stream = file.OpenReadStream();
            
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}"; 

            var etag = await _mediaService.UploadFileAsync(stream, uniqueFileName, bucketName, file.ContentType);

            return Ok(new { Message = "Upload successful", ETag = etag, FileName = uniqueFileName });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Media upload failed: {ex.Message}" });
        }
    }

    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl([FromQuery] string bucketName, [FromQuery] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "File key is required." });
        }

        try
        {
            var url = await _mediaService.GeneratePresignedUrlAsync(bucketName, key);
            return Ok(new { PresignedUrl = url });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error generating URL: {ex.Message}" });
        }
    }
}