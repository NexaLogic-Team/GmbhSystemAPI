namespace GmbhSystem.Domain.Entities;

public class HomeSection
{
    public int Id { get; set; }
    public string MainTitleEn { get; set; } = string.Empty;
    public string Description1En { get; set; } = string.Empty;
    public string MainTitleDe { get; set; } = string.Empty;
    public string Description1De { get; set; } = string.Empty;
    
    // Hero Background Media Fields
    public string HeroMediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image"; // "image" or "video"

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}