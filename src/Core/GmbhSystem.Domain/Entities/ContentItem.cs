namespace GmbhSystem.Domain.Entities;

public class ContentItem
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty; 
    public string Section { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}