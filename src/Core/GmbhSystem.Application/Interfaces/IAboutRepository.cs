using GmbhSystem.Application.Dtos;

namespace GmbhSystem.Application.Interfaces;

public interface IAboutRepository
{
    /// <summary>
    /// CMS Edit Form အတွက် EN + DE Content နှစ်ခုလုံးကို ယူရန်
    /// </summary>
    Task<AboutSectionDto> GetAboutSectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Public Page အတွက် သက်ဆိုင်ရာ Language (en / de) သီးသန့်ယူရန်
    /// </summary>
    Task<AboutSectionDto> GetAboutSectionByLangAsync(string lang, CancellationToken cancellationToken = default);

    /// <summary>
    /// CMS Edit Form မှ EN/DE Data များကို Save/Update ပြုလုပ်ရန်
    /// </summary>
    Task UpdateAboutSectionAsync(AboutSectionDto dto, CancellationToken cancellationToken = default);
}