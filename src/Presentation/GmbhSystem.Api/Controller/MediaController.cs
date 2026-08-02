using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 20MB
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName)
    {
        _logger.LogInformation(">>> [POST] api/cms/media/upload called. BucketName: '{BucketName}', File: '{FileName}'", bucketName, file?.FileName);

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning(">>> Upload failed: No file selected or file length is 0.");
            return BadRequest(new { message = "No file selected." });
        }

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            _logger.LogWarning(">>> Upload failed: Bucket name is null or empty.");
            return BadRequest(new { message = "Bucket name is required." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}"; 

            _logger.LogInformation(">>> Uploading file to Storage with key: '{Key}'...", uniqueFileName);
            var etag = await _mediaService.UploadFileAsync(stream, uniqueFileName, bucketName, file.ContentType);

            _logger.LogInformation(">>> Upload SUCCESS! Key: '{Key}', ETag: '{ETag}'", uniqueFileName, etag);
            return Ok(new { Message = "Upload successful", ETag = etag, FileName = uniqueFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> Upload EXCEPTION: {ErrorMessage}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Media upload failed: {ex.Message}" });
        }   
    }

    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl([FromQuery] string bucketName, [FromQuery] string key)
    {
        _logger.LogInformation(">>> [GET] api/cms/media/presigned-url called. Bucket: '{Bucket}', Key: '{Key}'", bucketName, key);

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning(">>> Presigned URL failed: File key is empty.");
            return BadRequest(new { message = "File key is required." });
        }

        try
        {
            var url = await _mediaService.GeneratePresignedUrlAsync(bucketName, key);
            _logger.LogInformation(">>> Generated Presigned URL successfully.");
            return Ok(new { PresignedUrl = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> Presigned URL EXCEPTION: {ErrorMessage}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error generating URL: {ex.Message}" });
        }
    }
    
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile([FromQuery] string bucketName, [FromQuery] string key)
    {
        _logger.LogInformation(">>> [DELETE] api/cms/media/delete called. Bucket: '{Bucket}', Key: '{Key}'", bucketName, key);

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(bucketName))
        {
            return BadRequest(new { message = "Bucket name and Key are required." });
        }

        try
        {
            await _mediaService.DeleteFileAsync(bucketName, key);
            return Ok(new { message = "File deleted successfully from R2." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> Delete File EXCEPTION: {ErrorMessage}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error deleting file: {ex.Message}" });
        }
    }
}