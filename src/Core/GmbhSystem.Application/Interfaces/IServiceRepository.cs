using GmbhSystem.Application.Dtos;
using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface IServiceRepository
{
    Task<List<ServiceItem>> GetAllAsync(string lang, CancellationToken cancellationToken = default);
    
    Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    Task AddAsync(ServiceItem service, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(ServiceItem service, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    Task UpdateSectionHeaderAsync(string subtitle, string mainTitle, string lang, CancellationToken cancellationToken = default);
    
    Task<HeaderResponseDto?> GetSectionHeaderAsync(string lang, CancellationToken cancellationToken = default);
}