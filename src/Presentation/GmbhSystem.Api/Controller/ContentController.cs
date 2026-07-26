using GmbhSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;
    
[Route("api/cms/content")]
[ApiController]
[Authorize]

public class ContentController : ControllerBase
{
    private readonly IContentRepository _contentRepository;

    public ContentController(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    [HttpGet("home")]
    public async Task<IActionResult> GetHomeContent([FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        var items = await _contentRepository.GetBySectionAndLanguageAsync("Home", lang, cancellationToken);
        
        var result = items.ToDictionary(k => k.Key, v => v.Value);
        return Ok(result);
    }

    [HttpPut("home")]
    public async Task<IActionResult> UpdateHomeContent([FromQuery] string lang, [FromBody] Dictionary<string, string> updates, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(lang))
        {
            return BadRequest(new { message = "Language parameter (lang) is required." });
        }

        var items = (await _contentRepository.GetBySectionAndLanguageAsync("Home", lang, cancellationToken)).ToList();

        foreach (var update in updates)
        {
            var item = items.FirstOrDefault(i => i.Key == update.Key);
            if (item != null)
            {
                item.Value = update.Value;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _contentRepository.UpdateRangeAsync(items, cancellationToken);
        return Ok(new { message = $"Home content for '{lang}' updated successfully" });
    }
}