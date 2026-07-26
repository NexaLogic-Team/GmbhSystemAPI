using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface IContentRepository
{
    Task<IEnumerable<ContentItem>> GetBySectionAndLanguageAsync(string section, string language, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<ContentItem> items, CancellationToken cancellationToken = default);
}