using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using GmbhSystem.Application.Dtos;
using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[Route("api/cms/leadership")]
[ApiController]
public class LeaderController : ControllerBase
{
    private readonly ILeaderRepository _leaderRepository;
    private readonly IMediaService _mediaService;

    public LeaderController(ILeaderRepository leaderRepository, IMediaService mediaService)
    {
        _leaderRepository = leaderRepository;
        _mediaService = mediaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaders([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var leaders = await _leaderRepository.GetAllAsync(normalizedLang, cancellationToken);
        return Ok(leaders ?? new List<LeaderItem>());
    }

    [HttpGet("{id:int}/detail")]
    public async Task<IActionResult> GetLeaderDetail(int id, CancellationToken cancellationToken = default)
    {
        var currentLeader = await _leaderRepository.GetByIdAsync(id, cancellationToken);
        if (currentLeader == null)
        {
            return NotFound(new { message = "Leader profile not found." });
        }

        var allEn = await _leaderRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _leaderRepository.GetAllAsync("de", cancellationToken);

        var leaderEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        var leaderDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        var result = new CreateLeaderDto
        {
            Name = currentLeader.Name,
            ImageUrl = currentLeader.ImageUrl,
            RoleEn = leaderEn?.Role ?? (currentLeader.Language == "en" ? currentLeader.Role : string.Empty),
            BioEn = leaderEn?.Bio ?? (currentLeader.Language == "en" ? currentLeader.Bio : string.Empty),
            RoleDe = leaderDe?.Role ?? (currentLeader.Language == "de" ? currentLeader.Role : string.Empty),
            BioDe = leaderDe?.Bio ?? (currentLeader.Language == "de" ? currentLeader.Bio : string.Empty)
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeader([FromBody] CreateLeaderDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request payload cannot be null." });
        }

        var enLeader = new LeaderItem
        {
            Id = 0,
            Name = request.Name,
            Role = request.RoleEn,
            Bio = request.BioEn,
            ImageUrl = request.ImageUrl,
            Language = "en"
        };
        await _leaderRepository.AddAsync(enLeader, cancellationToken);

        var deLeader = new LeaderItem
        {
            Id = 0,
            Name = request.Name,
            Role = request.RoleDe,
            Bio = request.BioDe,
            ImageUrl = request.ImageUrl,
            Language = "de"
        };
        await _leaderRepository.AddAsync(deLeader, cancellationToken);

        return Ok(new { message = "Leader profiles created successfully in EN and DE!" });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLeader(int id, [FromBody] CreateLeaderDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Invalid request payload." });
        }

        var current = await _leaderRepository.GetByIdAsync(id, cancellationToken);
        if (current == null)
        {
            return NotFound(new { message = "Leader profile not found." });
        }

        var allEn = await _leaderRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _leaderRepository.GetAllAsync("de", cancellationToken);

        var leaderEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(current.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        var leaderDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(current.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (leaderEn != null)
        {
            leaderEn.Name = request.Name;
            leaderEn.Role = request.RoleEn;
            leaderEn.Bio = request.BioEn;
            if (!string.IsNullOrEmpty(request.ImageUrl)) leaderEn.ImageUrl = request.ImageUrl;
            await _leaderRepository.UpdateAsync(leaderEn, cancellationToken);
        }

        if (leaderDe != null)
        {
            leaderDe.Name = request.Name;
            leaderDe.Role = request.RoleDe;
            leaderDe.Bio = request.BioDe;
            if (!string.IsNullOrEmpty(request.ImageUrl)) leaderDe.ImageUrl = request.ImageUrl;
            await _leaderRepository.UpdateAsync(leaderDe, cancellationToken);
        }

        return Ok(new { message = "Leader profile updated successfully in both EN and DE!" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLeader(int id, CancellationToken cancellationToken = default)
    {
        var currentLeader = await _leaderRepository.GetByIdAsync(id, cancellationToken);
        if (currentLeader == null)
        {
            return NotFound(new { message = "Leader profile not found." });
        }

        var allEn = await _leaderRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _leaderRepository.GetAllAsync("de", cancellationToken);

        var leaderEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        var leaderDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        var imageKey = currentLeader.ImageUrl ?? leaderEn?.ImageUrl ?? leaderDe?.ImageUrl;
        if (!string.IsNullOrEmpty(imageKey))
        {
            await _mediaService.DeleteFileAsync("gmbh", imageKey);
        }

        if (leaderEn != null)
        {
            await _leaderRepository.DeleteAsync(leaderEn.Id, cancellationToken);
        }

        if (leaderDe != null)
        {
            await _leaderRepository.DeleteAsync(leaderDe.Id, cancellationToken);
        }

        if ((leaderEn == null || leaderEn.Id != id) && (leaderDe == null || leaderDe.Id != id))
        {
            await _leaderRepository.DeleteAsync(id, cancellationToken);
        }

        return Ok(new { message = "Leader profile and image deleted successfully in both EN and DE!" });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetLanguageStatus(CancellationToken cancellationToken = default)
    {
        var enLeaders = await _leaderRepository.GetAllAsync("en", cancellationToken);
        var deLeaders = await _leaderRepository.GetAllAsync("de", cancellationToken);

        return Ok(new
        {
            HasEnglishData = enLeaders != null && enLeaders.Any(),
            HasGermanData = deLeaders != null && deLeaders.Any()
        });
    }

    [HttpPut("header")]
    public async Task<IActionResult> UpdateSectionHeader([FromBody] UpdateHeaderRequest request,
        [FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MainTitle))
        {
            return BadRequest(new { message = "Main Title is required." });
        }

        var normalizedLang = lang.ToLower();
        await _leaderRepository.UpdateSectionHeaderAsync(request.Subtitle, request.MainTitle, normalizedLang,
            cancellationToken);
        return Ok(new { message = $"Header updated successfully for {normalizedLang.ToUpper()}!" });
    }

    [HttpGet("header")]
    public async Task<IActionResult> GetSectionHeader([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var header = await _leaderRepository.GetSectionHeaderAsync(normalizedLang, cancellationToken);

        if (header == null)
        {
            return Ok(new
            {
                Subtitle = normalizedLang == "de" ? "VORSTAND" : "BOARD OF DIRECTORS",
                MainTitle = normalizedLang == "de" ? "Unsere Führungskräfte" : "Meet Our Leadership"
            });
        }

        return Ok(header);
    }
}