namespace GmbhSystem.Domain.Entities;

public class ContentItem
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty; // ဥပမာ - "home_hero_title"
    public string Value { get; set; } = string.Empty; // Website မှာ ပြမည့် Content စာသား
    public string Section { get; set; } = string.Empty; // ဥပမာ - "Home", "About", "Services"
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}