using GmbhSystem.Application.Dtos;

namespace GmbhSystem.Application.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
}