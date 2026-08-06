
using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[Route("api/public/services")]
[ApiController]
public class PublicServiceController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMediaService _mediaService;
    private const string BucketName = "gmbh"; // S3 / R2 Bucket Name

    public PublicServiceController(
        IServiceRepository serviceRepository, 
        IMediaService mediaService)
    {
        _serviceRepository = serviceRepository;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Get Services Section Header & Service List for Public Website
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetServiceSection([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();

        // 1. Header Info ယူမည်
        var header = await _serviceRepository.GetSectionHeaderAsync(normalizedLang, cancellationToken);

        // 2. Services list ယူမည်
        var services = await _serviceRepository.GetAllAsync(normalizedLang, cancellationToken);

        // 3. Image Key များကို Presigned URL သို့ ပြောင်းလဲမည်
        var serviceDtos = new List<ServiceItemDto>();

        if (services != null)
        {
            foreach (var item in services)
            {
                var imageUrl = item.ImageUrl;

                // Key သာ ဖြစ်ပြီး Complete URL (http...) မဟုတ်ပါက Presigned URL ထုတ်ပေးမည်
                if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        imageUrl = await _mediaService.GeneratePresignedUrlAsync(BucketName, imageUrl);
                    }
                    catch
                    {
                        // URL Generate မရပါက Original Key အတိုင်း ထားမည်
                    }
                }

                serviceDtos.Add(new ServiceItemDto
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    ImageUrl = imageUrl
                });
            }
        }

        var response = new ServiceHeaderDto
        {
            Subtitle = header?.Subtitle ?? (normalizedLang == "de" ? "BILATERALE EXPERTISE" : "BILATERAL EXPERTISE"),
            MainTitle = header?.MainTitle ?? (normalizedLang == "de" ? "Unsere Dienstleistungen" : "Our Services"),
            Services = serviceDtos
        };

        return Ok(response);
    }
}