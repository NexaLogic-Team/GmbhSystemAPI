using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using GmbhSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[Route("api/cms/services")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMediaService _mediaService;

    public ServiceController(IServiceRepository serviceRepository, IMediaService mediaService)
    {
        _serviceRepository = serviceRepository;
        _mediaService = mediaService;
    }

    /// <summary>
    /// GET API: Selected language (en / de) အလိုက် Service List ယူရန်
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetServices([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var services = await _serviceRepository.GetAllAsync(normalizedLang, cancellationToken);
        return Ok(services ?? new List<ServiceItem>());
    }

    /// <summary>
    /// GET API: Dual Language Form အတွက် Service detail (EN + DE) ယူရန်
    /// </summary>
    [HttpGet("{id:int}/detail")]
    public async Task<IActionResult> GetServiceDetail(int id, CancellationToken cancellationToken = default)
    {
        var currentService = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        if (currentService == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);

        var serviceEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Title.Trim().Equals(currentService.Title.Trim(), StringComparison.OrdinalIgnoreCase));
        var serviceDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Title.Trim().Equals(currentService.Title.Trim(), StringComparison.OrdinalIgnoreCase));

        var result = new CreateServiceDto
        {
            ImageUrl = currentService.ImageUrl,
            TitleEn = serviceEn?.Title ?? (currentService.Language == "en" ? currentService.Title : string.Empty),
            DescriptionEn = serviceEn?.Description ?? (currentService.Language == "en" ? currentService.Description : string.Empty),
            TitleDe = serviceDe?.Title ?? (currentService.Language == "de" ? currentService.Title : string.Empty),
            DescriptionDe = serviceDe?.Description ?? (currentService.Language == "de" ? currentService.Description : string.Empty)
        };

        return Ok(result);
    }

    /// <summary>
    /// CREATE API: Form တစ်ခုတည်းမှ EN ရော DE ပါ တစ်ပြိုင်နက် Create လုပ်ရန်
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request payload cannot be null." });
        }

        // 1. English Service Item Create
        var enService = new ServiceItem
        {
            Id = 0,
            Title = request.TitleEn,
            Description = request.DescriptionEn,
            ImageUrl = request.ImageUrl,
            Language = "en"
        };
        await _serviceRepository.AddAsync(enService, cancellationToken);

        // 2. German Service Item Create
        var deService = new ServiceItem
        {
            Id = 0,
            Title = request.TitleDe,
            Description = request.DescriptionDe,
            ImageUrl = request.ImageUrl,
            Language = "de"
        };
        await _serviceRepository.AddAsync(deService, cancellationToken);

        return Ok(new { message = "Service created successfully in EN and DE!" });
    }

    /// <summary>
    /// UPDATE API: Form တစ်ခုတည်းမှ EN ရော DE ပါ တစ်ပြိုင်နက် Update လုပ်ရန်
    /// </summary>
    // [HttpPut("{id:int}")]
    // public async Task<IActionResult> UpdateService(int id, [FromBody] CreateServiceDto request,
    //     CancellationToken cancellationToken = default)
    // {
    //     if (request == null)
    //     {
    //         return BadRequest(new { message = "Invalid request payload." });
    //     }
    //
    //     var current = await _serviceRepository.GetByIdAsync(id, cancellationToken);
    //     if (current == null)
    //     {
    //         return NotFound(new { message = "Service not found." });
    //     }
    //
    //     var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
    //     var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);
    //
    //     var serviceEn = allEn.FirstOrDefault(x =>
    //         x.Id == id || x.Title.Trim().Equals(current.Title.Trim(), StringComparison.OrdinalIgnoreCase));
    //     var serviceDe = allDe.FirstOrDefault(x =>
    //         x.Id == id || x.Title.Trim().Equals(current.Title.Trim(), StringComparison.OrdinalIgnoreCase));
    //
    //     // English Record Update
    //     if (serviceEn != null)
    //     {
    //         serviceEn.Title = request.TitleEn;
    //         serviceEn.Description = request.DescriptionEn;
    //         if (!string.IsNullOrEmpty(request.ImageUrl)) serviceEn.ImageUrl = request.ImageUrl;
    //         await _serviceRepository.UpdateAsync(serviceEn, cancellationToken);
    //     }
    //
    //     // German Record Update
    //     if (serviceDe != null)
    //     {
    //         serviceDe.Title = request.TitleDe;
    //         serviceDe.Description = request.DescriptionDe;
    //         if (!string.IsNullOrEmpty(request.ImageUrl)) serviceDe.ImageUrl = request.ImageUrl;
    //         await _serviceRepository.UpdateAsync(serviceDe, cancellationToken);
    //     }
    //
    //     return Ok(new { message = "Service updated successfully in both EN and DE!" });
    // }
    
    /// <summary>
/// UPDATE API: Form တစ်ခုတည်းမှ EN ရော DE ပါ တစ်ပြိုင်နက် Update လုပ်ရန်
/// </summary>
[HttpPut("{id:int}")]
public async Task<IActionResult> UpdateService(int id, [FromBody] CreateServiceDto request,
    CancellationToken cancellationToken = default)
{
    if (request == null)
    {
        return BadRequest(new { message = "Invalid request payload." });
    }

    var current = await _serviceRepository.GetByIdAsync(id, cancellationToken);
    if (current == null)
    {
        return NotFound(new { message = "Service not found." });
    }

    var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
    var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);

    // Current Service ၏ Title ဖြင့် EN / DE Mapping ရှာဖွေခြင်း
    var targetTitle = current.Title?.Trim();
    var serviceEn = allEn.FirstOrDefault(x =>
        x.Id == id || (!string.IsNullOrEmpty(targetTitle) && 
                       !string.IsNullOrEmpty(x.Title) && 
                       x.Title.Trim().Equals(targetTitle, StringComparison.OrdinalIgnoreCase)));

    var serviceDe = allDe.FirstOrDefault(x =>
        x.Id == id || (!string.IsNullOrEmpty(targetTitle) && 
                       !string.IsNullOrEmpty(x.Title) && 
                       x.Title.Trim().Equals(targetTitle, StringComparison.OrdinalIgnoreCase)));

    // 1. English Record Update (TitleEn ဖြည့်ထားမှသာ Title ကို ပြောင်းမည်၊ ကွက်လပ်ဖြစ်နေပါက မူလ Title အတိုင်း ထိန်းထားမည်)
    if (serviceEn != null)
    {
        if (!string.IsNullOrWhiteSpace(request.TitleEn))
        {
            serviceEn.Title = request.TitleEn;
        }
        serviceEn.Description = request.DescriptionEn;
        if (!string.IsNullOrEmpty(request.ImageUrl)) serviceEn.ImageUrl = request.ImageUrl;
        
        await _serviceRepository.UpdateAsync(serviceEn, cancellationToken);
    }

    // 2. German Record Update (TitleDe ဖြည့်ထားမှသာ Title ကို ပြောင်းမည်၊ ကွက်လပ်ဖြစ်နေပါက မူလ Title အတိုင်း ထိန်းထားမည်)
    if (serviceDe != null)
    {
        if (!string.IsNullOrWhiteSpace(request.TitleDe))
        {
            serviceDe.Title = request.TitleDe;
        }
        serviceDe.Description = request.DescriptionDe;
        if (!string.IsNullOrEmpty(request.ImageUrl)) serviceDe.ImageUrl = request.ImageUrl;
        
        await _serviceRepository.UpdateAsync(serviceDe, cancellationToken);
    }

    return Ok(new { message = "Service updated successfully in both EN and DE!" });
}

    // /// <summary>
    // /// DELETE API: EN/DE Records နှစ်ခုလုံးနှင့် R2 Media/Image ပါ ဖျက်ရန်
    // /// </summary>
    // [HttpDelete("{id:int}")]
    // public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken = default)
    // {
    //     var currentService = await _serviceRepository.GetByIdAsync(id, cancellationToken);
    //     if (currentService == null)
    //     {
    //         return NotFound(new { message = "Service not found." });
    //     }
    //
    //     var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
    //     var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);
    //
    //     var serviceEn = allEn.FirstOrDefault(x =>
    //         x.Id == id || x.Title.Trim().Equals(currentService.Title.Trim(), StringComparison.OrdinalIgnoreCase));
    //
    //     var serviceDe = allDe.FirstOrDefault(x =>
    //         x.Id == id || x.Title.Trim().Equals(currentService.Title.Trim(), StringComparison.OrdinalIgnoreCase));
    //
    //     // 1. Media Service ကိုသုံးပြီး R2 ပေါ်မှ Image ကို ဖျက်ခြင်း
    //     var imageKey = currentService.ImageUrl ?? serviceEn?.ImageUrl ?? serviceDe?.ImageUrl;
    //     if (!string.IsNullOrEmpty(imageKey))
    //     {
    //         await _mediaService.DeleteFileAsync("gmbh", imageKey);
    //     }
    //
    //     // 2. English Record ရှိပါက Delete လုပ်မည်
    //     if (serviceEn != null)
    //     {
    //         await _serviceRepository.DeleteAsync(serviceEn.Id, cancellationToken);
    //     }
    //
    //     // 3. German Record ရှိပါက Delete လုပ်မည်
    //     if (serviceDe != null)
    //     {
    //         await _serviceRepository.DeleteAsync(serviceDe.Id, cancellationToken);
    //     }
    //
    //     if ((serviceEn == null || serviceEn.Id != id) && (serviceDe == null || serviceDe.Id != id))
    //     {
    //         await _serviceRepository.DeleteAsync(id, cancellationToken);
    //     }
    //
    //     return Ok(new { message = "Service and image deleted successfully in both EN and DE!" });
    // }
    
    /// <summary>
    /// DELETE API: EN/DE Records နှစ်ခုလုံးနှင့် R2 Media/Image ပါ ဖျက်ရန်
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken = default)
    {
        var currentService = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        if (currentService == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);

        var serviceEn = allEn.FirstOrDefault(x =>
            x.Id == id || x.Title.Trim().Equals(currentService.Title.Trim(), StringComparison.OrdinalIgnoreCase));

        var serviceDe = allDe.FirstOrDefault(x =>
            x.Id == id || x.Title.Trim().Equals(currentService.Title.Trim(), StringComparison.OrdinalIgnoreCase));

        // 1. Media Service ကိုသုံးပြီး R2 ပေါ်မှ Image ကို ဖျက်ခြင်း
        var imageKey = currentService.ImageUrl ?? serviceEn?.ImageUrl ?? serviceDe?.ImageUrl;
        if (!string.IsNullOrEmpty(imageKey))
        {
            await _mediaService.DeleteFileAsync("gmbh", imageKey);
        }

        // 2. English Record ရှိပါက Delete လုပ်မည်
        if (serviceEn != null)
        {
            await _serviceRepository.DeleteAsync(serviceEn.Id, cancellationToken);
        }

        // 3. German Record ရှိပါက Delete လုပ်မည်
        if (serviceDe != null)
        {
            await _serviceRepository.DeleteAsync(serviceDe.Id, cancellationToken);
        }

        if ((serviceEn == null || serviceEn.Id != id) && (serviceDe == null || serviceDe.Id != id))
        {
            await _serviceRepository.DeleteAsync(id, cancellationToken);
        }

        return Ok(new { message = "Service and image deleted successfully in both EN and DE!" });
    }

    /// <summary>
    /// GET API: Language Status တင်ထားခြင်း ရှိ/မရှိ စစ်ရန်
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetLanguageStatus(CancellationToken cancellationToken = default)
    {
        var enServices = await _serviceRepository.GetAllAsync("en", cancellationToken);
        var deServices = await _serviceRepository.GetAllAsync("de", cancellationToken);

        return Ok(new
        {
            HasEnglishData = enServices != null && enServices.Any(),
            HasGermanData = deServices != null && deServices.Any()
        });
    }

    /// <summary>
    /// PUT API: Section Header Settings ပြင်ရန်
    /// </summary>
    [HttpPut("header")]
    public async Task<IActionResult> UpdateSectionHeader([FromBody] UpdateHeaderRequest request,
        [FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MainTitle))
        {
            return BadRequest(new { message = "Main Title is required." });
        }

        var normalizedLang = lang.ToLower();
        await _serviceRepository.UpdateSectionHeaderAsync(request.Subtitle, request.MainTitle, normalizedLang,
            cancellationToken);
        return Ok(new { message = $"Header updated successfully for {normalizedLang.ToUpper()}!" });
    }

    /// <summary>
    /// GET API: Section Header Settings ယူရန်
    /// </summary>
    [HttpGet("header")]
    public async Task<IActionResult> GetSectionHeader([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var header = await _serviceRepository.GetSectionHeaderAsync(normalizedLang, cancellationToken);

        if (header == null)
        {
            return Ok(new
            {
                Subtitle = normalizedLang == "de" ? "BILATERALE EXPERTISE" : "BILATERAL EXPERTISE",
                MainTitle = normalizedLang == "de" ? "Unsere Dienstleistungen" : "Our Services"
            });
        }

        return Ok(header);
    }
}