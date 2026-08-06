using GmbhSystem.Infrastructure.Services;
using GmbhSystem.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Api.Controller.Web;

[Route("api/public/home")]
[ApiController]
public class PublicHomeController : ControllerBase
{
    private readonly GmbhSystemDbContext _context;
    private readonly IMediaService _mediaService;
    private const string BucketName = "gmbh"; // S3 / Cloudflare R2 Bucket Name

    public PublicHomeController(GmbhSystemDbContext context, IMediaService mediaService)
    {
        _context = context;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Get Home / Hero Section Data for Public Website
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHomeSection([FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var home = await _context.HomeSections.FirstOrDefaultAsync(cancellationToken);

        if (home == null)
        {
            return Ok(new PublicContentController.HomeSectionPublicDto
            {
                MainTitle = normalizedLang == "de"
                    ? "Deutschland und Myanmar durch Wirtschaft, Handel und Innovation verbinden"
                    : "Connecting Germany and Myanmar Through Business, Trade, and Innovation",
                Description = normalizedLang == "de"
                    ? "Die Irrawaddy GmbH ist ein in München ansässiges Unternehmen..."
                    : "Irrawaddy GmbH is a Munich-based company dedicated to creating business opportunities between Germany and Myanmar.",
                HeroMediaUrl = "",
                MediaType = "image"
            });
        }

        var heroUrl = home.HeroMediaUrl;

        // Bucket Key သာဖြစ်ပါက Presigned URL ပြောင်းပေးမည်
        if (!string.IsNullOrEmpty(heroUrl) && !heroUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                heroUrl = await _mediaService.GeneratePresignedUrlAsync(BucketName, heroUrl);
            }
            catch
            {
                // URL Generate မရပါက Key အတိုင်း ထားမည်
            }
        }

        var response = new PublicContentController.HomeSectionPublicDto
        {
            MainTitle = normalizedLang == "de" ? home.MainTitleDe : home.MainTitleEn,
            Description = normalizedLang == "de" ? home.Description1De : home.Description1En,
            HeroMediaUrl = heroUrl,
            MediaType = home.MediaType
        };

        return Ok(response);
    }
}