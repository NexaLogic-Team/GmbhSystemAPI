using GmbhSystem.Application.Dtos;
using GmbhSystem.Domain.Entities;
using GmbhSystem.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/home")]
public class HomeCmsController : ControllerBase
{
    private readonly GmbhSystemDbContext _context;

    public HomeCmsController(GmbhSystemDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets current Home Section Hero data (or public display).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<HomeSectionDto>> GetHomeSection()
    {
        var home = await _context.HomeSections.FirstOrDefaultAsync();

        if (home == null)
        {
            return Ok(new HomeSectionDto
            {
                MainTitleEn = "Connecting Germany and Myanmar Through Business, Trade, and Innovation",
                Description1En = "Irrawaddy GmbH is a Munich-based company dedicated to creating business opportunities between Germany and Myanmar.",
                MainTitleDe = "Deutschland und Myanmar durch Wirtschaft, Handel und Innovation verbinden",
                Description1De = "Die Irrawaddy GmbH ist ein in München ansässiges Unternehmen...",
                HeroMediaUrl = "",
                MediaType = "image"
            });
        }

        return Ok(new HomeSectionDto
        {
            MainTitleEn = home.MainTitleEn,
            Description1En = home.Description1En,
            MainTitleDe = home.MainTitleDe,
            Description1De = home.Description1De,
            HeroMediaUrl = home.HeroMediaUrl,
            MediaType = home.MediaType
        });
    }

    /// <summary>
    /// Updates or creates the single Home Section entry.
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateHomeSection([FromBody] HomeSectionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var home = await _context.HomeSections.FirstOrDefaultAsync();

        if (home == null)
        {
            home = new HomeSection
            {
                MainTitleEn = dto.MainTitleEn,
                Description1En = dto.Description1En,
                MainTitleDe = dto.MainTitleDe,
                Description1De = dto.Description1De,
                HeroMediaUrl = dto.HeroMediaUrl,
                MediaType = dto.MediaType,
                UpdatedAt = DateTime.UtcNow
            };
            _context.HomeSections.Add(home);
        }
        else
        {
            home.MainTitleEn = dto.MainTitleEn;
            home.Description1En = dto.Description1En;
            home.MainTitleDe = dto.MainTitleDe;
            home.Description1De = dto.Description1De;
            home.HeroMediaUrl = dto.HeroMediaUrl;
            home.MediaType = dto.MediaType;
            home.UpdatedAt = DateTime.UtcNow;
            _context.HomeSections.Update(home);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}