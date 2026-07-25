using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GmbhSystem.Api.Controller.Cms;

[Route("api/web/[controller]")]
[ApiController]
public class WebContentController : ControllerBase
{
    // Public Website မှ Data လှမ်းယူရန် Endpoints များ
    [HttpGet]
    public async Task<IActionResult> GetPublishedContent()
    {
        // Fetch published content for the website
        return Ok(new { Message = "Public website content data" });
    }
}