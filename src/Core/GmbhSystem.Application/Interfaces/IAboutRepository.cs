using GmbhSystem.Application.Dtos;

namespace GmbhSystem.Application.Interfaces;

public interface IAboutRepository
{
    Task<AboutSectionDto> GetAboutSectionAsync(CancellationToken cancellationToken = default);
    Task<AboutSectionDto> GetAboutSectionByLangAsync(string lang, CancellationToken cancellationToken = default);
    Task UpdateAboutSectionAsync(AboutSectionDto dto, CancellationToken cancellationToken = default);
}