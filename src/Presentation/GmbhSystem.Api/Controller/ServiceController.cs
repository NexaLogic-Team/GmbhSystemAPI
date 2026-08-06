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

    [HttpGet]
    public async Task<IActionResult> GetServices([FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var services = await _serviceRepository.GetAllAsync(normalizedLang, cancellationToken);
        return Ok(services ?? new List<ServiceItem>());
    }


    [HttpGet("{id:int}/detail")]
    public async Task<IActionResult> GetServiceDetail(int id, CancellationToken cancellationToken = default)
    {
        var current = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        if (current == null) return NotFound(new { message = "Service not found." });

        // Fetch all records to map EN and DE pair
        var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);

        ServiceItem? serviceEn = null;
        ServiceItem? serviceDe = null;

        if (current.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            serviceEn = current;
            // If ImageUrl matches or position matches, pair it (or default to current)
            serviceDe = allDe.FirstOrDefault(x => x.ImageUrl == current.ImageUrl)
                        ?? allDe.FirstOrDefault(x => x.Id == id);
        }
        else
        {
            serviceDe = current;
            serviceEn = allEn.FirstOrDefault(x => x.ImageUrl == current.ImageUrl)
                        ?? allEn.FirstOrDefault(x => x.Id == id);
        }

        var result = new CreateServiceDto
        {
            TitleEn = serviceEn?.Title ?? string.Empty,
            DescriptionEn = serviceEn?.Description ?? string.Empty,
            TitleDe = serviceDe?.Title ?? string.Empty,
            DescriptionDe = serviceDe?.Description ?? string.Empty,
            ImageUrl = current.ImageUrl ?? string.Empty
        };

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] CreateServiceDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Invalid request payload." });
        }

        var service = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        if (service == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        // Determine target language from entity or DTO payload
        if (service.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(request.TitleEn))
            {
                service.Title = request.TitleEn;
            }

            service.Description = request.DescriptionEn ?? string.Empty;
        }
        else if (service.Language.Equals("de", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(request.TitleDe))
            {
                service.Title = request.TitleDe;
            }

            service.Description = request.DescriptionDe ?? string.Empty;
        }

        // Update image if a new image was uploaded
        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            service.ImageUrl = request.ImageUrl;
        }

        await _serviceRepository.UpdateAsync(service, cancellationToken);

        return Ok(new { message = $"Service updated successfully for {service.Language.ToUpper()}!" });
    }

    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request payload cannot be null." });
        }

        var enService = new ServiceItem
        {
            Id = 0,
            Title = request.TitleEn,
            Description = request.DescriptionEn,
            ImageUrl = request.ImageUrl,
            Language = "en"
        };
        await _serviceRepository.AddAsync(enService, cancellationToken);

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken = default)
    {
        // 1. Get the current item being deleted
        var currentItem = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        if (currentItem == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        // 2. Fetch all services across both languages
        var allEn = await _serviceRepository.GetAllAsync("en", cancellationToken);
        var allDe = await _serviceRepository.GetAllAsync("de", cancellationToken);

        // 3. Find matching records across EN and DE (matched by primary key or shared ImageUrl)
        var itemEn = allEn.FirstOrDefault(x =>
            x.Id == id || (!string.IsNullOrEmpty(currentItem.ImageUrl) && x.ImageUrl == currentItem.ImageUrl));
        var itemDe = allDe.FirstOrDefault(x =>
            x.Id == id || (!string.IsNullOrEmpty(currentItem.ImageUrl) && x.ImageUrl == currentItem.ImageUrl));

        // Fallback if no cross-match by ImageUrl: use current item
        if (currentItem.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            itemEn ??= currentItem;
        }
        else
        {
            itemDe ??= currentItem;
        }

        // 4. Delete English record if present
        if (itemEn != null)
        {
            await _serviceRepository.DeleteAsync(itemEn.Id, cancellationToken);
        }

        // 5. Delete German record if present
        if (itemDe != null && itemDe.Id != itemEn?.Id)
        {
            await _serviceRepository.DeleteAsync(itemDe.Id, cancellationToken);
        }

        return Ok(new { message = "Service deleted successfully in both EN and DE!" });
    }

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