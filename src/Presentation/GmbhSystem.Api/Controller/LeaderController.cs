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
    private readonly IMediaService _mediaService; // Inject MediaService

    public LeaderController(ILeaderRepository leaderRepository, IMediaService mediaService)
    {
        _leaderRepository = leaderRepository;
        _mediaService = mediaService;
    }

    /// <summary>
    /// GET API: Selected language (en / de) အလိုက် Leader List ယူရန်
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLeaders([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var leaders = await _leaderRepository.GetAllAsync(normalizedLang, cancellationToken);
        return Ok(leaders ?? new List<LeaderItem>());
    }

    /// <summary>/// <summary>
    /// GET API: Dual Language Form အတွက် Leader detail (EN + DE) ယူရန်
    /// </summary>
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

        // Name ဖြင့် သို့မဟုတ် ID ဖြင့် EN/DE ယှဉ်ရှာခြင်း
        var leaderEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        var leaderDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        var result = new CreateLeaderDto
        {
            Name = currentLeader.Name,
            ImageUrl = currentLeader.ImageUrl, // Profile Image Key / Path
            RoleEn = leaderEn?.Role ?? (currentLeader.Language == "en" ? currentLeader.Role : string.Empty),
            BioEn = leaderEn?.Bio ?? (currentLeader.Language == "en" ? currentLeader.Bio : string.Empty),
            RoleDe = leaderDe?.Role ?? (currentLeader.Language == "de" ? currentLeader.Role : string.Empty),
            BioDe = leaderDe?.Bio ?? (currentLeader.Language == "de" ? currentLeader.Bio : string.Empty)
        };

        return Ok(result);
    }

    /// <summary>
    /// CREATE API: Form တစ်ခုတည်းမှ EN ရော DE ပါ တစ်ပြိုင်နက် Create လုပ်ရန်
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLeader([FromBody] CreateLeaderDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request payload cannot be null." });
        }

        // 1. English Leader Item Create
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

        // 2. German Leader Item Create
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

    /// <summary>
    /// UPDATE API: Form တစ်ခုတည်းမှ EN ရော DE ပါ တစ်ပြိုင်နက် Update လုပ်ရန်
    /// </summary>
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

        // EN နဲ့ DE Records များကို DB မှ ရှာယူခြင်း
        var allEn = await _leaderRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _leaderRepository.GetAllAsync("de", cancellationToken);

        var leaderEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(current.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        var leaderDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Name.Trim().Equals(current.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        // English Record ကို Update လုပ်ခြင်း
        if (leaderEn != null)
        {
            leaderEn.Name = request.Name;
            leaderEn.Role = request.RoleEn;
            leaderEn.Bio = request.BioEn;
            if (!string.IsNullOrEmpty(request.ImageUrl)) leaderEn.ImageUrl = request.ImageUrl;
            await _leaderRepository.UpdateAsync(leaderEn, cancellationToken);
        }

        // German Record ကို Update လုပ်ခြင်း
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

    // [HttpDelete("{id:int}")]
    // public async Task<IActionResult> DeleteLeader(int id, CancellationToken cancellationToken = default)
    // {
    //     var existing = await _leaderRepository.GetByIdAsync(id, cancellationToken);
    //     if (existing == null)
    //     {
    //         return NotFound(new { message = "Leader not found." });
    //     }
    //
    //     await _leaderRepository.DeleteAsync(id, cancellationToken);
    //     return Ok(new { message = "Leader deleted successfully." });
    // }

    // [HttpDelete("{id:int}")]
    // public async Task<IActionResult> DeleteLeader(int id, CancellationToken cancellationToken = default)
    // {
    //     // 1. ဖျက်မည့် Target Leader ကို ယူမည်
    //     var currentLeader = await _leaderRepository.GetByIdAsync(id, cancellationToken);
    //     if (currentLeader == null)
    //     {
    //         return NotFound(new { message = "Leader profile not found." });
    //     }
    //
    //     // 2. EN နှင့် DE Data အားလုံးကို ရှာယူမည်
    //     var allEn = await _leaderRepository.GetAllAsync("en", cancellationToken);
    //     var allDe = await _leaderRepository.GetAllAsync("de", cancellationToken);
    //
    //     // 3. Name တူညီသော (သို့မဟုတ် ID တူညီသော) EN / DE Record နှစ်ခုစလုံး၏ ID များကို ရှာမည်
    //     var leaderEn = allEn.FirstOrDefault(x =>
    //         x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));
    //
    //     var leaderDe = allDe.FirstOrDefault(x =>
    //         x.Id == id || x.Name.Trim().Equals(currentLeader.Name.Trim(), StringComparison.OrdinalIgnoreCase));
    //
    //     // 4. English Record ရှိပါက Delete လုပ်မည်
    //     if (leaderEn != null)
    //     {
    //         await _leaderRepository.DeleteAsync(leaderEn.Id, cancellationToken);
    //     }
    //
    //     // 5. German Record ရှိပါက Delete လုပ်မည်
    //     if (leaderDe != null)
    //     {
    //         await _leaderRepository.DeleteAsync(leaderDe.Id, cancellationToken);
    //     }
    //
    //     // သီးခြား Link မရှိဘဲ ကျန်ခဲ့နိုင်သော တိုက်ရိုက် ရွေးချယ်ထားသည့် Record ကိုလည်း သေချာအောင် ဖျက်မည်
    //     if ((leaderEn == null || leaderEn.Id != id) && (leaderDe == null || leaderDe.Id != id))
    //     {
    //         await _leaderRepository.DeleteAsync(id, cancellationToken);
    //     }
    //
    //     return Ok(new { message = "Leader profile deleted successfully in both EN and DE!" });
    // }
    
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

        // 1. R2 ထဲက သက်ဆိုင်ရာ Image Key ကို ယူပြီး ဖျက်မည်
        var imageKey = currentLeader.ImageUrl ?? leaderEn?.ImageUrl ?? leaderDe?.ImageUrl;
        if (!string.IsNullOrEmpty(imageKey))
        {
            // URL direct ဖြစ်နေပါက Key ဖြတ်ယူခြင်း သို့မဟုတ် Key သီးသန့်ဖြစ်ပါက တိုက်ရိုက်ဖျက်ခြင်း
            await _mediaService.DeleteFileAsync("gmbh", imageKey);
        }

        // 2. English Record ရှိပါက Delete လုပ်မည်
        if (leaderEn != null)
        {
            await _leaderRepository.DeleteAsync(leaderEn.Id, cancellationToken);
        }

        // 3. German Record ရှိပါက Delete လုပ်မည်
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