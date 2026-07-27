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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tempFilePath = Path.GetTempFileName();
        using (var stream = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        try
        {
            var etag = await _mediaService.UploadFileAsync(tempFilePath, bucketName);

            System.IO.File.Delete(tempFilePath);

            return Ok(new { Message = "Upload successful", ETag = etag, FileName = file.FileName });
        }
        catch (Exception ex)
        {
            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);

            return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        }
    }

    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl([FromQuery] string bucketName, [FromQuery] string key)
    {
        var url = await _mediaService.GeneratePresignedUrlAsync(bucketName, key);
        return Ok(new { PresignedUrl = url });
    }
}