using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence.Repositories;

public class AboutRepository : IAboutRepository
{
    private readonly GmbhSystemDbContext _context;
    private const string SECTION_NAME = "About";

    public AboutRepository(GmbhSystemDbContext context)
    {
        _context = context;
    }

    public async Task<AboutSectionDto> GetAboutSectionAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.ContentItems
            .Where(c => c.Section == SECTION_NAME)
            .ToListAsync(cancellationToken);

        var enItems = items.Where(x => x.Language == "en").ToList();
        var deItems = items.Where(x => x.Language == "de").ToList();

        return new AboutSectionDto
        {
            SubTitleEn = GetValue(enItems, "SubTitle"),
            MainTitleEn = GetValue(enItems, "MainTitle"),
            Paragraph1En = GetValue(enItems, "Paragraph1"),
            Paragraph2En = GetValue(enItems, "Paragraph2"),
            Paragraph3En = GetValue(enItems, "Paragraph3"),
            Paragraph4En = GetValue(enItems, "Paragraph4"),

            SubTitleDe = GetValue(deItems, "SubTitle"),
            MainTitleDe = GetValue(deItems, "MainTitle"),
            Paragraph1De = GetValue(deItems, "Paragraph1"),
            Paragraph2De = GetValue(deItems, "Paragraph2"),
            Paragraph3De = GetValue(deItems, "Paragraph3"),
            Paragraph4De = GetValue(deItems, "Paragraph4"),

            ImageUrl = GetValue(items, "ImageUrl")
        };
    }

    public async Task<AboutSectionDto> GetAboutSectionByLangAsync(string lang,
        CancellationToken cancellationToken = default)
    {
        var normalizedLang = string.IsNullOrWhiteSpace(lang) ? "en" : lang.ToLower();

        var items = await _context.ContentItems
            .Where(c => c.Section == SECTION_NAME && (c.Language == normalizedLang || c.Key == "ImageUrl"))
            .ToListAsync(cancellationToken);

        return new AboutSectionDto
        {
            SubTitleEn = GetValue(items, "SubTitle"),
            MainTitleEn = GetValue(items, "MainTitle"),
            Paragraph1En = GetValue(items, "Paragraph1"),
            Paragraph2En = GetValue(items, "Paragraph2"),
            Paragraph3En = GetValue(items, "Paragraph3"),
            Paragraph4En = GetValue(items, "Paragraph4"),
            ImageUrl = GetValue(items, "ImageUrl")
        };
    }

    public async Task UpdateAboutSectionAsync(AboutSectionDto dto, CancellationToken cancellationToken = default)
    {
        var existingItems = await _context.ContentItems
            .Where(c => c.Section == SECTION_NAME)
            .ToListAsync(cancellationToken);

        // English items mapping
        UpsertContentItem(existingItems, "SubTitle", dto.SubTitleEn, "en");
        UpsertContentItem(existingItems, "MainTitle", dto.MainTitleEn, "en");
        UpsertContentItem(existingItems, "Paragraph1", dto.Paragraph1En, "en");
        UpsertContentItem(existingItems, "Paragraph2", dto.Paragraph2En, "en");
        UpsertContentItem(existingItems, "Paragraph3", dto.Paragraph3En, "en");
        UpsertContentItem(existingItems, "Paragraph4", dto.Paragraph4En, "en");

        // German items mapping
        UpsertContentItem(existingItems, "SubTitle", dto.SubTitleDe, "de");
        UpsertContentItem(existingItems, "MainTitle", dto.MainTitleDe, "de");
        UpsertContentItem(existingItems, "Paragraph1", dto.Paragraph1De, "de");
        UpsertContentItem(existingItems, "Paragraph2", dto.Paragraph2De, "de");
        UpsertContentItem(existingItems, "Paragraph3", dto.Paragraph3De, "de");
        UpsertContentItem(existingItems, "Paragraph4", dto.Paragraph4De, "de");

        // Common Image Key mapping
        UpsertContentItem(existingItems, "ImageUrl", dto.ImageUrl, "en");

        await _context.SaveChangesAsync(cancellationToken);
    }

    #region Helper Methods

    private string GetValue(IEnumerable<ContentItem> items, string key)
    {
        return items.FirstOrDefault(x => x.Key == key)?.Value ?? string.Empty;
    }

    private void UpsertContentItem(List<ContentItem> existingItems, string key, string value, string language)
    {
        var existing = existingItems.FirstOrDefault(x => x.Key == key && x.Language == language);

        if (existing != null)
        {
            existing.Value = value ?? string.Empty;
            _context.ContentItems.Update(existing);
        }
        else
        {
            _context.ContentItems.Add(new ContentItem
            {
                Section = SECTION_NAME,
                Key = key,
                Value = value ?? string.Empty,
                Language = language
            });
        }
    }

    #endregion
}