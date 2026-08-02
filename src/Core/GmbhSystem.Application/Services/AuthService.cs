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

    public async Task<bool> ChangePasswordAsync(string email, ChangePasswordDto request,
        CancellationToken cancellationToken)
    {
        // 1. Email ဖြင့် User ကို ရှာပါ
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        // 2. လက်ရှိ Password မှန်/မမှန် BCrypt ဖြင့် စစ်ဆေးပါ
        bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            return false;
        }

        // 3. Password အသစ်ကို BCrypt ဖြင့် Hash လုပ်ပြီး သိမ်းဆည်းပါ
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // User Update database call (User entity ကို DB မှာ update လုပ်သည့် logic)
        await _userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }
}