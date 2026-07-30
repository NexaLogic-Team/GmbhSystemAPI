using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface ILeaderRepository
{
    // Leaders CRUD
    Task<IEnumerable<LeaderItem>> GetAllAsync(string language, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaderItem>> GetAllOrderedAsync(string language, CancellationToken cancellationToken = default);
    Task<LeaderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LeaderItem> AddAsync(LeaderItem leader, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaderItem leader, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Section Header Methods
    Task<LeadershipHeader?> GetSectionHeaderAsync(string language, CancellationToken cancellationToken = default);

    Task UpdateSectionHeaderAsync(string subtitle, string mainTitle, string language,
        CancellationToken cancellationToken = default);
}