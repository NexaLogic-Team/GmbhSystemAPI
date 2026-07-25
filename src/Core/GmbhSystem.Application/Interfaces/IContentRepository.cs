using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface IContentRepository
{
    Task<ContentItem?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<ContentItem>> GetBySectionAsync(string section, CancellationToken cancellationToken = default);
    Task UpdateAsync(ContentItem contentItem, CancellationToken cancellationToken = default);
}