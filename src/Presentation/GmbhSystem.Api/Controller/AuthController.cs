using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller;

[ApiController]
[Route("api/cms/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    // Fixed: Changed Task to Task<IActionResult>
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var token = await _authService.LoginAsync(request, cancellationToken);
        
        if (token == null)
        {
            return Unauthorized(new { Message = "Invalid username or password." });
        }

        return Ok(new { Token = token });
    }
}