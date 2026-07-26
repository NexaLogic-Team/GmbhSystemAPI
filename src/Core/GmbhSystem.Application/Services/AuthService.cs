using GmbhSystem.Application.Dtos; // Note: Ensure this matches the namespace in your LoginDto.cs
using GmbhSystem.Application.Interfaces;

namespace GmbhSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUserRepository userRepository, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<string?> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null; 
        }

        return _jwtProvider.Generate(user);
    }
}