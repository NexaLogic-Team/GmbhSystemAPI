using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly GmbhSystemDbContext _context;

    public ServiceRepository(GmbhSystemDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceItem>> GetAllAsync(string lang, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceItems
            .Where(x => x.Language.ToLower() == lang.ToLower())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceItems.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(ServiceItem service, CancellationToken cancellationToken = default)
    {
        service.CreatedAt = DateTime.UtcNow;
        _context.ServiceItems.Add(service);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceItem service, CancellationToken cancellationToken = default)
    {
        service.UpdatedAt = DateTime.UtcNow;
        _context.ServiceItems.Update(service);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item != null)
        {
            _context.ServiceItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Section Header Logic Implementation matching LeaderRepository pattern
    public async Task<HeaderResponseDto?> GetSectionHeaderAsync(string lang, CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var header = await _context.ServiceHeaders
            .FirstOrDefaultAsync(h => h.Language.ToLower() == normalizedLang, cancellationToken);

        if (header == null) return null;

        return new HeaderResponseDto
        {
            Subtitle = header.Subtitle,
            MainTitle = header.MainTitle
        };
    }

    public async Task UpdateSectionHeaderAsync(string subtitle, string mainTitle, string lang, CancellationToken cancellationToken = default)
    {
        var normalizedLang = lang.ToLower();
        var existing = await _context.ServiceHeaders
            .FirstOrDefaultAsync(h => h.Language.ToLower() == normalizedLang, cancellationToken);

        if (existing != null)
        {
            existing.Subtitle = subtitle;
            existing.MainTitle = mainTitle;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.ServiceHeaders.Update(existing);
        }
        else
        {
            var newHeader = new ServiceHeader
            {
                Subtitle = subtitle,
                MainTitle = mainTitle,
                Language = normalizedLang,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ServiceHeaders.Add(newHeader);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}