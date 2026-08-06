using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Infrastructure.Services;
using GmbhSystem.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Api.Controller.Web;

[Route("api/public/content")]
[ApiController]
public class PublicContentController : ControllerBase
{
    private readonly GmbhSystemDbContext _context;
    private readonly IServiceRepository _serviceRepository;
    private readonly ILeaderRepository _leaderRepository;
    private readonly IAboutRepository _aboutRepository;
    private readonly IMediaService _mediaService;
    private const string BucketName = "gmbh";

    public PublicContentController(
        GmbhSystemDbContext context,
        IServiceRepository serviceRepository,
        ILeaderRepository leaderRepository,
        IAboutRepository aboutRepository,
        IMediaService mediaService)
    {
        _context = context;
        _serviceRepository = serviceRepository;
        _leaderRepository = leaderRepository;
        _aboutRepository = aboutRepository;
        _mediaService = mediaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicContent([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();

        // ------------------------------------------
        // 1. Home Section
        // ------------------------------------------
        var home = await _context.HomeSections.FirstOrDefaultAsync(cancellationToken);
        HomeSectionPublicDto homeData;

        if (home == null)
        {
            homeData = new HomeSectionPublicDto
            {
                MainTitle = normalizedLang == "de"
                    ? "Deutschland und Myanmar durch Wirtschaft, Handel und Innovation verbinden"
                    : "Connecting Germany and Myanmar Through Business, Trade, and Innovation",
                Description = normalizedLang == "de"
                    ? "Die Irrawaddy GmbH ist ein in München ansässiges Unternehmen..."
                    : "Irrawaddy GmbH is a Munich-based company dedicated to creating business opportunities between Germany and Myanmar.",
                HeroMediaUrl = "",
                MediaType = "image"
            };
        }
        else
        {
            var heroUrl = await ResolvePresignedUrlAsync(home.HeroMediaUrl);
            homeData = new HomeSectionPublicDto
            {
                MainTitle = normalizedLang == "de" ? home.MainTitleDe : home.MainTitleEn,
                Description = normalizedLang == "de" ? home.Description1De : home.Description1En,
                HeroMediaUrl = heroUrl,
                MediaType = home.MediaType
            };
        }

        // ------------------------------------------
        // 2. Services Section
        // ------------------------------------------
        var serviceHeader = await _serviceRepository.GetSectionHeaderAsync(normalizedLang, cancellationToken);
        var services = await _serviceRepository.GetAllAsync(normalizedLang, cancellationToken);
        var serviceDtos = new List<ServiceItemDto>();

        if (services != null)
        {
            foreach (var item in services)
            {
                var imageUrl = await ResolvePresignedUrlAsync(item.ImageUrl);
                serviceDtos.Add(new ServiceItemDto
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    ImageUrl = imageUrl
                });
            }
        }

        var servicesData = new ServiceSectionDto
        {
            Subtitle = serviceHeader?.Subtitle ??
                       (normalizedLang == "de" ? "BILATERALE EXPERTISE" : "BILATERAL EXPERTISE"),
            MainTitle = serviceHeader?.MainTitle ??
                        (normalizedLang == "de" ? "Unsere Dienstleistungen" : "Our Services"),
            Services = serviceDtos
        };

        // ------------------------------------------
        // 3. Leadership Section
        // ------------------------------------------
        var leaderHeader = await _leaderRepository.GetSectionHeaderAsync(normalizedLang, cancellationToken);
        var leaders = await _leaderRepository.GetAllOrderedAsync(normalizedLang, cancellationToken);
        var leaderDtos = new List<LeaderItemDto>();

        if (leaders != null)
        {
            foreach (var x in leaders)
            {
                var imageUrl = await ResolvePresignedUrlAsync(x.ImageUrl);
                leaderDtos.Add(new LeaderItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Role = x.Role,
                    Bio = x.Bio,
                    ImageUrl = imageUrl,
                    DisplayOrder = x.DisplayOrder
                });
            }
        }

        var leadershipData = new LeadershipSectionDto
        {
            Subtitle = leaderHeader?.Subtitle ?? (normalizedLang == "de" ? "VORSTAND" : "BOARD OF DIRECTORS"),
            MainTitle = leaderHeader?.MainTitle ??
                        (normalizedLang == "de" ? "Unsere Führungskräfte" : "Meet Our Leadership"),
            Leaders = leaderDtos
        };

        // ------------------------------------------
        // 4. About Us Section
        // ------------------------------------------
        var aboutRaw = await _aboutRepository.GetAboutSectionAsync(cancellationToken);
        AboutPublicDto aboutData;

        if (aboutRaw == null)
        {
            aboutData = new AboutPublicDto();
        }
        else
        {
            var aboutImageUrl = await ResolvePresignedUrlAsync(aboutRaw.ImageUrl);

            var subtitle = normalizedLang == "de" ? aboutRaw.SubTitleDe : aboutRaw.SubTitleEn;
            var mainTitle = normalizedLang == "de" ? aboutRaw.MainTitleDe : aboutRaw.MainTitleEn;

            var p1 = normalizedLang == "de" ? aboutRaw.Paragraph1De : aboutRaw.Paragraph1En;
            var p2 = normalizedLang == "de" ? aboutRaw.Paragraph2De : aboutRaw.Paragraph2En;
            var p3 = normalizedLang == "de" ? aboutRaw.Paragraph3De : aboutRaw.Paragraph3En;
            var p4 = normalizedLang == "de" ? aboutRaw.Paragraph4De : aboutRaw.Paragraph4En;

            var fullDescription =
                string.Join("\n\n", new[] { p1, p2, p3, p4 }.Where(p => !string.IsNullOrWhiteSpace(p)));

            aboutData = new AboutPublicDto
            {
                Subtitle = subtitle,
                MainTitle = mainTitle,
                Description = fullDescription,
                ImageUrl = aboutImageUrl
            };
        }

        // ------------------------------------------
        // Return Combined Response
        // ------------------------------------------
        var response = new PublicCombinedContentDto
        {
            Home = homeData,
            Services = servicesData,
            Leadership = leadershipData,
            AboutUs = aboutData
        };

        return Ok(response);
    }

    /// <summary>
    /// Helper Method to Convert S3/R2 Key into Dynamic Presigned URL
    /// </summary>
    private async Task<string> ResolvePresignedUrlAsync(string? keyOrUrl)
    {
        if (string.IsNullOrWhiteSpace(keyOrUrl)) return string.Empty;

        if (keyOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return keyOrUrl;
        }

        try
        {
            return await _mediaService.GeneratePresignedUrlAsync(BucketName, keyOrUrl);
        }
        catch
        {
            return keyOrUrl;
        }
    }

    public class PublicCombinedContentDto
    {
        public HomeSectionPublicDto Home { get; set; } = new();
        public ServiceSectionDto Services { get; set; } = new();
        public LeadershipSectionDto Leadership { get; set; } = new();
        public AboutPublicDto AboutUs { get; set; } = new();
    }

    public class HomeSectionPublicDto
    {
        public string MainTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HeroMediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
    }

    public class AboutPublicDto
    {
        public string Subtitle { get; set; } = string.Empty;
        public string MainTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class ServiceSectionDto
    {
        public string Subtitle { get; set; } = string.Empty;
        public string MainTitle { get; set; } = string.Empty;
        public List<ServiceItemDto> Services { get; set; } = new();
    }

    public class ServiceItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class LeadershipSectionDto
    {
        public string Subtitle { get; set; } = string.Empty;
        public string MainTitle { get; set; } = string.Empty;
        public List<LeaderItemDto> Leaders { get; set; } = new();
    }

    public class LeaderItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}