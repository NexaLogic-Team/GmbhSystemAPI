namespace GmbhSystem.Domain.Entities;

public class LeaderItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public int DisplayOrder { get; set; } = 0; // Display Order
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class LeadershipSectionDto
{
    public string Subtitle { get; set; } = "BOARD OF DIRECTORS";
    public string MainTitle { get; set; } = "Meet Our Leadership";
    public List<LeaderItemDto> Leaders { get; set; } = new();
}

public class LeaderItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class UpdateHeaderRequest
{
    public string Subtitle { get; set; } = string.Empty;
    public string MainTitle { get; set; } = string.Empty;
}

public class LeadershipHeader
{
    public int Id { get; set; }
    public string Subtitle { get; set; } = "BOARD OF DIRECTORS";
    public string MainTitle { get; set; } = "Meet Our Leadership";
    public string Language { get; set; } = "en";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}