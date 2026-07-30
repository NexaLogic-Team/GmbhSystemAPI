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

    // [HttpPost("upload")]
    // public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName)
    // {
    //     if (file == null || file.Length == 0)
    //         return BadRequest("No file uploaded.");
    //
    //     var tempFilePath = Path.GetTempFileName();
    //     using (var stream = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Create))
    //     {
    //         await file.CopyToAsync(stream);
    //     }
    //
    //     try
    //     {
    //         var etag = await _mediaService.UploadFileAsync(tempFilePath, bucketName);
    //
    //         System.IO.File.Delete(tempFilePath);
    //
    //         return Ok(new { Message = "Upload successful", ETag = etag, FileName = file.FileName });
    //     }
    //     catch (Exception ex)
    //     {
    //         if (System.IO.File.Exists(tempFilePath))
    //             System.IO.File.Delete(tempFilePath);
    //
    //         return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
    //     }
    // }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
    
        // Original file extension ကို ထုတ်ယူခြင်း (ဥပမာ: .jpg, .png)
        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
    
        var tempFilePath = Path.GetTempFileName();
        using (var stream = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
    
        try
        {
            // _mediaService ထဲသို့ file ရဲ့ original name, content type များကိုပါ ပို့ပေးရန် လိုအပ်သည်
            var etag = await _mediaService.UploadFileAsync(tempFilePath, bucketName, uniqueFileName, file.ContentType);
    
            System.IO.File.Delete(tempFilePath);
    
            // Frontend က သိမ်းမယ့် FileName သည် uniqueFileName ဖြစ်ရပါမည် (သို့မှသာ R2 ထဲက key နဲ့ တူမည်)
            return Ok(new { Message = "Upload successful", ETag = etag, FileName = uniqueFileName });
        }
        catch (Exception ex)
        {
            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);
    
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        }
    }
    
    // [HttpPost("upload")]
    // public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName)
    // {
    //     if (file == null || file.Length == 0)
    //         return BadRequest("No file uploaded.");
    //
    //     // Original file extension ကို ထုတ်ယူခြင်း (ဥပမာ: .jpg, .png)
    //     var extension = Path.GetExtension(file.FileName);
    //     var uniqueFileName = $"{Guid.NewGuid()}{extension}";
    //
    //     var tempFilePath = Path.GetTempFileName();
    //     using (var stream = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Create))
    //     {
    //         await file.CopyToAsync(stream);
    //     }
    //
    //     try
    //     {
    //         // MediaService ထتهသို့ uniqueFileName နှင့် ContentType ပါ ပို့ပေးရန်
    //         var etag = await _mediaService.UploadFileAsync(tempFilePath, bucketName, uniqueFileName, file.ContentType);
    //
    //         System.IO.File.Delete(tempFilePath);
    //
    //         return Ok(new { Message = "Upload successful", ETag = etag, FileName = uniqueFileName });
    //     }
    //     catch (Exception ex)
    //     {
    //         if (System.IO.File.Exists(tempFilePath))
    //             System.IO.File.Delete(tempFilePath);
    //
    //         return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
    //     }
    // }

    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl([FromQuery] string bucketName, [FromQuery] string key)
    {
        var url = await _mediaService.GeneratePresignedUrlAsync(bucketName, key);
        return Ok(new { PresignedUrl = url });
    }
}