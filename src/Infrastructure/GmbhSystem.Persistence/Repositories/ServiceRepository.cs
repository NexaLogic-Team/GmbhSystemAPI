using GmbhSystem.Application.Dtos;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly GmbhSystemDbContext _context; // သင်၏ DbContext Name ထည့်ပါ

    public ServiceRepository(GmbhSystemDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceItem>> GetAllAsync(string lang, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ServiceItem>()
            .Where(x => x.Language.ToLower() == lang.ToLower())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ServiceItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ServiceItem service, CancellationToken cancellationToken = default)
    {
        await _context.Set<ServiceItem>().AddAsync(service, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceItem service, CancellationToken cancellationToken = default)
    {
        service.UpdatedAt = DateTime.UtcNow;
        _context.Set<ServiceItem>().Update(service);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item != null)
        {
            _context.Set<ServiceItem>().Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSectionHeaderAsync(string subtitle, string mainTitle, string lang,
        CancellationToken cancellationToken = default)
    {
        // Section Header Table / Key-Value Table တစ်ခုခုတွင် သိမ်းဆည်းသည့် Logic
        // တကယ်လို့ DB Table ထဲသိမ်းတာဆိုရင် EF Core Save logic ထည့်ပေးရပါမည်
    }

    public async Task<HeaderResponseDto?> GetSectionHeaderAsync(string lang,
        CancellationToken cancellationToken = default)
    {
        // DB မှ Header settings ပြန်ထုတ်သည့် Logic
        return null; // Header Entity မရှိသေးပါက Controller ထဲက Default fallback value ယူပါမည်
    }
}