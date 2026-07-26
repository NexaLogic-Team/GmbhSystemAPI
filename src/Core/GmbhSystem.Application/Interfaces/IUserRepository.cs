using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}