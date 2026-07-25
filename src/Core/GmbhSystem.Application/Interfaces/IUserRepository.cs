using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}