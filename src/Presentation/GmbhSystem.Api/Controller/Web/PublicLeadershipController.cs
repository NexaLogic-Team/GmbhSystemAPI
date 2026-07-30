using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller.Web;

[Route("api/public/leadership")]
[ApiController]
public class PublicLeadershipController : ControllerBase
{
    private readonly ILeaderRepository _leaderRepository;

    public PublicLeadershipController(ILeaderRepository leaderRepository)
    {
        _leaderRepository = leaderRepository;
    }

    /// <summary>
    /// Get Leadership Section Header & Leaders List for Public Website
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLeadershipSection([FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        // Header Info
        var header = await _leaderRepository.GetSectionHeaderAsync(lang, cancellationToken);
        
        // Leaders list (DisplayOrder အတိုင်း စီပြီးသား)
        var leaders = await _leaderRepository.GetAllOrderedAsync(lang, cancellationToken);

        var response = new LeadershipSectionDto
        {
            Subtitle = header?.Subtitle ?? "BOARD OF DIRECTORS",
            MainTitle = header?.MainTitle ?? "Meet Our Leadership",
            // Leaders = leaders
        };

        return Ok(response);
    }
}