namespace GmbhSystem.Application.Dtos;

public class ServiceHeaderDto
{
    public string Subtitle { get; set; } = string.Empty;
    public string MainTitle { get; set; } = string.Empty;
    public List<ServiceItemDto> Services { get; set; }
}

public class HeaderResponseDto
{
    public string Subtitle { get; set; } = string.Empty;
    public string MainTitle { get; set; } = string.Empty;
}

public class ServiceItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Language { get; set; } = "en"; // "en" or "de"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}