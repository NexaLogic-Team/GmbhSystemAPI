using GmbhSystem.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller.Cms;

[Authorize(Roles = "Admin")]
[Route("api/cms/[controller]")]
[ApiController]
public class CmsController : ControllerBase
{
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContent(int id, [FromBody] UpdateContentDto model)
    {
        // Business logic implementation
        return NoContent();
    }
}