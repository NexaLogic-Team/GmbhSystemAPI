using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly GmbhSystemDbContext _context;

    public ContentRepository(GmbhSystemDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<ContentItem>> GetBySectionAndLanguageAsync(string section, string language, CancellationToken cancellationToken = default)
    {
        return await _context.ContentItems
            .Where(c => c.Section == section && c.Language == language)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<ContentItem> items, CancellationToken cancellationToken = default)
    {
        _context.ContentItems.UpdateRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }
}