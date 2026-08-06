using GmbhSystem.Application.Interfaces;
using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller.Web;

[Route("api/public/about")]
[ApiController]
public class PublicAboutController : ControllerBase
{
    private readonly IAboutRepository _aboutRepository;
    private readonly IMediaService _mediaService;
    private const string BucketName = "gmbh"; // S3 / Cloudflare R2 Bucket Name

    public PublicAboutController(IAboutRepository aboutRepository, IMediaService mediaService)
    {
        _aboutRepository = aboutRepository;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Get About Us Section Data for Public Website
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAboutSection([FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var aboutRaw = await _aboutRepository.GetAboutSectionAsync(cancellationToken);

        if (aboutRaw == null)
        {
            return Ok(new PublicContentController.AboutPublicDto());
        }

        var imageUrl = aboutRaw.ImageUrl;

        // Image Key သာဖြစ်ပါက Presigned URL သို့ ပြောင်းလဲပေးမည်
        if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                imageUrl = await _mediaService.GeneratePresignedUrlAsync(BucketName, imageUrl);
            }
            catch
            {
                // URL Generate မရပါက Key အတိုင်း ထားမည်
            }
        }

        // Language အလိုက် Subtitle, MainTitle နှင့် Paragraph များကို ရွေးထုတ်ခြင်း
        var subtitle = normalizedLang == "de" ? aboutRaw.SubTitleDe : aboutRaw.SubTitleEn;
        var mainTitle = normalizedLang == "de" ? aboutRaw.MainTitleDe : aboutRaw.MainTitleEn;

        var p1 = normalizedLang == "de" ? aboutRaw.Paragraph1De : aboutRaw.Paragraph1En;
        var p2 = normalizedLang == "de" ? aboutRaw.Paragraph2De : aboutRaw.Paragraph2En;
        var p3 = normalizedLang == "de" ? aboutRaw.Paragraph3De : aboutRaw.Paragraph3En;
        var p4 = normalizedLang == "de" ? aboutRaw.Paragraph4De : aboutRaw.Paragraph4En;

        // Paragraph များကို Full String အဖြစ် ပေါင်းစပ်ခြင်း
        var fullDescription = string.Join("\n\n", new[] { p1, p2, p3, p4 }.Where(p => !string.IsNullOrWhiteSpace(p)));

        var response = new PublicContentController.AboutPublicDto
        {
            Subtitle = subtitle,
            MainTitle = mainTitle,
            Description = fullDescription,
            ImageUrl = imageUrl
        };

        return Ok(response);
    }
}