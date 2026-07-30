using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface ILeaderRepository
{
    Task<IEnumerable<LeaderItem>> GetAllAsync(string language, CancellationToken cancellationToken = default);
    Task<LeaderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LeaderItem> AddAsync(LeaderItem leader, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaderItem leader, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}