using GmbhSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Api.Controller;

[Route("api/cms/leadership")]
[ApiController]
// [Authorize]
public class LeaderController : ControllerBase
{
    private readonly ILeaderRepository _leaderRepository;

    public LeaderController(ILeaderRepository leaderRepository)
    {
        _leaderRepository = leaderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaders([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var leaders = await _leaderRepository.GetAllAsync(lang, cancellationToken);
        return Ok(leaders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeader([FromBody] LeaderItem leader, [FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        leader.Language = lang;
        var created = await _leaderRepository.AddAsync(leader, cancellationToken);
        return Ok(created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLeader(int id, [FromBody] LeaderItem leader,
        CancellationToken cancellationToken = default)
    {
        if (id != leader.Id)
        {
            return BadRequest(new { message = "ID mismatch." });
        }

        var existing = await _leaderRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Leader not found." });
        }

        existing.Name = leader.Name;
        existing.Role = leader.Role;
        existing.Bio = leader.Bio;
        existing.ImageUrl = leader.ImageUrl;

        await _leaderRepository.UpdateAsync(existing, cancellationToken);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLeader(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _leaderRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Leader not found." });
        }

        await _leaderRepository.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Leader deleted successfully." });
    }

    [HttpPut("header")]
    public async Task<IActionResult> UpdateSectionHeader([FromBody] UpdateHeaderRequest request,
        [FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.MainTitle))
        {
            return BadRequest(new { message = "Main Title is required." });
        }

        try
        {
            await _leaderRepository.UpdateSectionHeaderAsync(request.Subtitle, request.MainTitle, lang,
                cancellationToken);
            return Ok(new { message = "Header updated successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = $"Error updating header: {ex.Message}" });
        }
    }
    
    [HttpGet("header")]
    public async Task<IActionResult> GetSectionHeader([FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        var header = await _leaderRepository.GetSectionHeaderAsync(lang, cancellationToken);
        if (header == null)
        {
            return Ok(new { Subtitle = "BOARD OF DIRECTORS", MainTitle = "Meet Our Leadership" });
        }
        return Ok(header);
    }
}