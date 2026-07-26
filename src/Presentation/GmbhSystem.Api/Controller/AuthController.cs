using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository) 
    {
        _authService = authService;
        _userRepository = userRepository;
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

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(new
        {
            fullName = user.FullName,
            username = user.Username,
            email = user.Email,
            role = user.Role
        });
    }
}