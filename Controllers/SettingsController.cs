using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;
using WebAppBackend.ViewModels;
using WebAppBackend.ViewModels.Settings;

namespace WebAppBackend.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {

        private readonly ISettingsProvider _settings;
        private readonly ApplicationDbContext _context;

        public SettingsController(ISettingsProvider settings, ApplicationDbContext context)
        {
            _settings = settings;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _settings.GetAsync();

            var model = new SettingsViewModel
            {
                Modules =

                [

                    new()
                    {
                        Name = "Branding",
                        Description = "Manage application name, images and identity.",
                        Icon = "bi bi-brush",
                        Action = "Branding",

                        Summary = new BrandingViewModel
                        {
                            Id = settings.Branding.Id,
                            AppName = settings.Branding.AppName,
                            Description = settings.Branding.Description,
                            LoginImageAsset = settings.Branding.LoginImageAsset,
                            FaviconAsset = settings.Branding.FaviconAsset,
                            HomescreenAsset = settings.Branding.HomescreenAsset,
                        }
                    },

                   new()
                    {
                        Name = "Social Media",
                        Description = "Manage social media links and profiles.",
                        Icon = "bi bi-share",
                        Action = "SocialMedia",
                        Summary = new SocialMediaViewModel
                        {
                            Id = settings.SocialMedia.Id,

                            HeaderText = settings.SocialMedia.HeaderText,

                            InstagramVisible = settings.SocialMedia.InstagramVisible,
                            InstagramUrl = settings.SocialMedia.InstagramUrl,

                            FacebookVisible = settings.SocialMedia.FacebookVisible,
                            FacebookUrl = settings.SocialMedia.FacebookUrl,

                            TwitterVisible = settings.SocialMedia.TwitterVisible,
                            TwitterUrl = settings.SocialMedia.TwitterUrl
                        }
                   },

                   ]
            };


            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> Branding()
        {
            var settings = await _settings.GetAsync();

            var model = new BrandingViewModel
            {
                Id = settings.Branding.Id,
                AppName = settings.Branding.AppName,
                Description = settings.Branding.Description,

                LoginImageAssetId = settings.Branding.LoginImageAssetId,
                LoginImageAsset = settings.Branding.LoginImageAsset,

                FaviconAssetId = settings.Branding.FaviconAssetId,
                FaviconAsset = settings.Branding.FaviconAsset,

                HomescreenAssetId = settings.Branding.HomescreenAssetId,
                HomescreenAsset = settings.Branding.HomescreenAsset,

                AvailableAssets = await _context.Assets
                    .Where(x => x.Type == AssetType.Image)
                    .ToListAsync()
            };

            return View("Branding/Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Branding(BrandingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableAssets = await _context.Assets
                    .Where(x => x.Type == AssetType.Image)
                    .ToListAsync();

                return View("Branding/Edit", model);
            }

            var settings = await _settings.GetAsync();

            settings.Branding.AppName = model.AppName;
            settings.Branding.Description = model.Description;
            settings.Branding.LoginImageAssetId = model.LoginImageAssetId;
            settings.Branding.FaviconAssetId = model.FaviconAssetId;
            settings.Branding.HomescreenAssetId = model.HomescreenAssetId;

            await _settings.UpdateAsync(settings);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SocialMedia()
        {
            var settings = await _settings.GetAsync();

            var model = new SocialMediaViewModel
            {
                Id = settings.SocialMedia.Id,

                HeaderText = settings.SocialMedia.HeaderText,

                InstagramVisible = settings.SocialMedia.InstagramVisible,
                InstagramUrl = settings.SocialMedia.InstagramUrl,

                FacebookVisible = settings.SocialMedia.FacebookVisible,
                FacebookUrl = settings.SocialMedia.FacebookUrl,

                TwitterVisible = settings.SocialMedia.TwitterVisible,
                TwitterUrl = settings.SocialMedia.TwitterUrl
            };

            return View("SocialMedia/Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SocialMedia(SocialMediaViewModel model)
        {
            if (!ModelState.IsValid)
                return View("SocialMedia/Edit", model);


            var settings = await _settings.GetAsync();

            settings.SocialMedia.HeaderText = model.HeaderText;

            settings.SocialMedia.InstagramVisible = model.InstagramVisible;
            settings.SocialMedia.InstagramUrl = model.InstagramUrl;

            settings.SocialMedia.FacebookVisible = model.FacebookVisible;
            settings.SocialMedia.FacebookUrl = model.FacebookUrl;

            settings.SocialMedia.TwitterVisible = model.TwitterVisible;
            settings.SocialMedia.TwitterUrl = model.TwitterUrl;


            await _settings.UpdateAsync(settings);

            return RedirectToAction(nameof(Index));
        }

    }
}

