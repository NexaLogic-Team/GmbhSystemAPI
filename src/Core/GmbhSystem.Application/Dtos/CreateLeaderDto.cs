namespace GmbhSystem.Application.Dtos;

public class CreateLeaderDto
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    // English Content
    public string RoleEn { get; set; } = string.Empty;
    public string BioEn { get; set; } = string.Empty;

    // German Content
    public string RoleDe { get; set; } = string.Empty;
    public string BioDe { get; set; } = string.Empty;
}