using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/about")]
public class AboutController : ControllerBase
{
    private readonly IAboutRepository _aboutRepository;
    private readonly IMediaService _mediaService;
    private readonly ILogger<AboutController> _logger;

    public AboutController(IAboutRepository aboutRepository, IMediaService mediaService, ILogger<AboutController> logger)
    {
        _aboutRepository = aboutRepository;
        _mediaService = mediaService;
        _logger = logger;
    }

    /// <summary>
    /// Dual Language Form အတွက် About Content တစ်ခုလုံးယူရန်
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAboutSection(CancellationToken cancellationToken = default)
    {
        var result = await _aboutRepository.GetAboutSectionAsync(cancellationToken);
        
        // ImageKey ကို Presigned URL ပြောင်းပေးရန် (လိုအပ်ပါက)
        if (!string.IsNullOrWhiteSpace(result?.ImageUrl) && !result.ImageUrl.StartsWith("http"))
        {
            try
            {
                result.ImageUrl = await _mediaService.GeneratePresignedUrlAsync("gmbh", result.ImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate presigned URL for About image.");
            }
        }

        return Ok(result ?? new AboutSectionDto());
    }

    /// <summary>
    /// EN + DE Content များကို တစ်ပြိုင်နက် Update ပြုလုပ်ရန်
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateAboutSection([FromBody] AboutSectionDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Invalid request payload." });
        }

        await _aboutRepository.UpdateAboutSectionAsync(request, cancellationToken);
        return Ok(new { message = "About Us section updated successfully!" });
    }
}