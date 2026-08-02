using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller.Web;

[Route("api/public/leadership")]
[ApiController]
public class PublicLeadershipController : ControllerBase
{
    private readonly ILeaderRepository _leaderRepository;
    private readonly IMediaService _mediaService;
    private const string BucketName = "gmbh"; // S3 / R2 Bucket Name

    public PublicLeadershipController(
        ILeaderRepository leaderRepository, 
        IMediaService mediaService)
    {
        _leaderRepository = leaderRepository;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Get Leadership Section Header & Leaders List for Public Website
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLeadershipSection([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        // 1. Header Info ယူမည်
        var header = await _leaderRepository.GetSectionHeaderAsync(lang, cancellationToken);

        // 2. Leaders list (DisplayOrder အတိုင်း စီပြီးသား) ယူမည်
        var leaders = await _leaderRepository.GetAllOrderedAsync(lang, cancellationToken);

        // 3. Image Key များကို Presigned URL သို့ ပြောင်းလဲမည်
        var leaderDtos = new List<LeaderItemDto>();

        foreach (var x in leaders)
        {
            var imageUrl = x.ImageUrl;

            // Key သာ ဖြစ်ပြီး Complete URL (http...) မဟုတ်ပါက Presigned URL ထုတ်ပေးမည်
            if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    imageUrl = await _mediaService.GeneratePresignedUrlAsync(BucketName, imageUrl);
                }
                catch
                {
                    // URL Generate မရပါက Orignal key အတိုင်း ထားမည်
                }
            }

            leaderDtos.Add(new LeaderItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Role = x.Role,
                Bio = x.Bio, // Bio ပါ ပို့ပေးရန် လိုအပ်ပါက
                ImageUrl = imageUrl,
                DisplayOrder = x.DisplayOrder,
            });
        }

        var response = new LeadershipSectionDto
        {
            Subtitle = header?.Subtitle ?? (lang.ToLower() == "de" ? "VORSTAND" : "BOARD OF DIRECTORS"),
            MainTitle = header?.MainTitle ?? (lang.ToLower() == "de" ? "Unsere Führungskräfte" : "Meet Our Leadership"),
            Leaders = leaderDtos
        };

        return Ok(response);
    }
}