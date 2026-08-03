namespace GmbhSystem.Application.Dtos;

public class CreateServiceDto
{
    public string TitleEn { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string TitleDe { get; set; } = string.Empty;
    public string DescriptionDe { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}