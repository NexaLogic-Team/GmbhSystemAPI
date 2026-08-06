namespace GmbhSystem.Application.Dtos;

public class HomeSectionDto
{
    public string MainTitleEn { get; set; } = string.Empty;
    public string Description1En { get; set; } = string.Empty;
    public string MainTitleDe { get; set; } = string.Empty;
    public string Description1De { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image"; // "image" or "video"
    public string HeroMediaUrl { get; set; } // "image" or "video"
}