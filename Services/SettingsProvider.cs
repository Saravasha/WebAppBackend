using Microsoft.EntityFrameworkCore;
using WebAppBackend.Data;
using WebAppBackend.Models;

namespace WebAppBackend.Services
{
    public interface ISettingsProvider
    {
        Task<Settings> GetAsync();

        Task UpdateAsync(Settings settings);

        Task<Settings> GetOrCreateAsync();

    }

    public class SettingsProvider : ISettingsProvider
    {
        private readonly ApplicationDbContext _context;

        public SettingsProvider(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Settings> GetAsync()
        {

            return await _context.Settings
                .Include(x => x.Branding)
                    .ThenInclude(x => x.LoginImageAsset)
                .Include(x => x.Branding)
                    .ThenInclude(x => x.FaviconAsset)
                .Include(x => x.Branding)
                    .ThenInclude(x => x.HomescreenAsset)
                .Include(x => x.SocialMedia)
                .SingleAsync();
        }

        public async Task<Settings> GetOrCreateAsync()
        {
            var settings = await _context.Settings.SingleOrDefaultAsync();

            if (settings == null)
            {
                settings = new Settings();

                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task UpdateAsync(Settings settings)
        {
            var existing = await _context.Settings
                .Include(x => x.Branding)
                .Include(x => x.SocialMedia)
                .SingleAsync();

            existing.Branding.AppName = settings.Branding.AppName;
            existing.Branding.Description = settings.Branding.Description;
            existing.Branding.LoginImageAssetId = settings.Branding.LoginImageAssetId;
            existing.Branding.FaviconAssetId = settings.Branding.FaviconAssetId;
            existing.Branding.HomescreenAssetId = settings.Branding.HomescreenAssetId;

            await _context.SaveChangesAsync();
        }
    }
}
