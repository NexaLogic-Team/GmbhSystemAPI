using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GmbhSystem.Infrastructure.Services;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IMediaService _mediaService;

    public AuthController(IAuthService authService, IUserRepository userRepository, IMediaService mediaService)
    {
        _authService = authService;
        _userRepository = userRepository;
        _mediaService = mediaService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _authService.LoginAsync(request, cancellationToken);

        if (token is null)
        {
            return Unauthorized(new { Message = "Invalid username or password." });
        }

        return Ok(new { Token = token });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email)?.Value
                    ?? User.Claims.FirstOrDefault(c => c.Type == "email" || c.Type.Contains("emailaddress"))?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "Email claim not found in the provided token." });
        }

        string bucketName = "gmbh";

        string jsonKey = $"profiles/{email}.json";
        string imageKey = $"{email}.png";

        string? profileImageUrl = null;
        try
        {
            profileImageUrl = await _mediaService.GeneratePresignedUrlAsync(bucketName, imageKey);
        }
        catch
        {
            profileImageUrl = null;
        }

        var jsonContent = await _mediaService.GetContentAsync(bucketName, jsonKey);

        if (string.IsNullOrEmpty(jsonContent))
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                fullName = user.FullName,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                profileImage = profileImageUrl
            });
        }

        var profileObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);

        return Ok(new
        {
            fullName = profileObj.TryGetProperty("fullName", out var fn) ? fn.GetString() : "",
            username = profileObj.TryGetProperty("username", out var un) ? un.GetString() : "",
            email = email,
            role = profileObj.TryGetProperty("role", out var r) ? r.GetString() : "",
            profileImage = profileImageUrl
        });
    }
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // JWT Token ထဲမှ Logged-in email ကို ရယူခြင်း
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email)?.Value
                    ?? User.Claims.FirstOrDefault(c => c.Type == "email" || c.Type.Contains("emailaddress"))?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "Token ထဲတွင် Email Claim မတွေ့ရှိပါ။" });
        }

        // Password ပြောင်းလဲခြင်း Logic ခေါ်ယူခြင်း
        var result = await _authService.ChangePasswordAsync(email, request, cancellationToken);

        if (!result)
        {
            return BadRequest(new { message = "လက်ရှိ စကားဝှက် မှားယွင်းနေပါသည် သို့မဟုတ် အသုံးပြုသူ မတွေ့ပါ။" });
        }

        return Ok(new { message = "စကားဝှက် အောင်မြင်စွာ ပြောင်းလဲပြီးပါပြီ။" });
    }
}