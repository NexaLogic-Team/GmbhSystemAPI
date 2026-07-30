using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence.Repositories;

public class LeaderRepository : ILeaderRepository
{
    private readonly GmbhSystemDbContext _context;

    public LeaderRepository(GmbhSystemDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderItem>> GetAllAsync(string language,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeaderItems
            .Where(l => l.Language == language)
            .OrderBy(l => l.DisplayOrder) // DisplayOrder မရှိသေးရင် Id အလိုက် စီပေးပါမယ်
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaderItem>> GetAllOrderedAsync(string language,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeaderItems
            .Where(l => l.Language == language)
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.LeaderItems.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<LeaderItem> AddAsync(LeaderItem leader, CancellationToken cancellationToken = default)
    {
        leader.CreatedAt = DateTime.UtcNow;
        _context.LeaderItems.Add(leader);
        await _context.SaveChangesAsync(cancellationToken);
        return leader;
    }

    public async Task UpdateAsync(LeaderItem leader, CancellationToken cancellationToken = default)
    {
        leader.UpdatedAt = DateTime.UtcNow;
        _context.LeaderItems.Update(leader);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _context.LeaderItems.FindAsync(new object[] { id }, cancellationToken);
        if (item != null)
        {
            _context.LeaderItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Section Header Logic Implementation
    public async Task<LeadershipHeader?> GetSectionHeaderAsync(string language,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeadershipHeaders
            .FirstOrDefaultAsync(h => h.Language == language, cancellationToken);
    }

    public async Task UpdateSectionHeaderAsync(string subtitle, string mainTitle, string language,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.LeadershipHeaders
            .FirstOrDefaultAsync(h => h.Language == language, cancellationToken);

        if (existing != null)
        {
            existing.Subtitle = subtitle;
            existing.MainTitle = mainTitle;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.LeadershipHeaders.Update(existing);
        }
        else
        {
            var newHeader = new LeadershipHeader
            {
                Subtitle = subtitle,
                MainTitle = mainTitle,
                Language = language,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LeadershipHeaders.Add(newHeader);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}