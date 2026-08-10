using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebAppBackend.Models;
using WebAppBackend.Data;
using WebAppBackend.Services;


namespace WebAppBackend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ReactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISettingsProvider _settings;
        public ReactController(ApplicationDbContext context, ISettingsProvider settings)
        {
            _context = context;
            _settings = settings;
        }

        [HttpGet("assets")]
        public async Task<ActionResult<List<Asset>>> GetAssets()
        {
            var assets = await _context.Assets
                .Where(a => a.Type != AssetType.Font)
                .Include(a => a.Categories)
                .ToListAsync();

            return Ok(assets);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<Category>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Assets)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("pages")]
        public async Task<ActionResult<List<Page>>> GetPages()
        {
            var pages = await _context.Pages
                .OrderBy(p => p.Order)
                .Select(p => new Page
                {
                    Id = p.Id,
                    Title = p.Title,
                    Container = p.Container,

                    Chapters = p.Chapters
                        .OrderBy(c => c.Order)
                        .Select(c => new Chapter
                        {
                            Id = c.Id,
                            Title = c.Title,
                            Container = c.Container,
                            Date = c.Date,
                            Order = c.Order,

                            Contents = c.Contents
                                .OrderBy(content => content.Order)
                                .Select(content => new Content
                                {
                                    Id = content.Id,
                                    Title = content.Title,
                                    Container = content.Container,
                                    Date = content.Date,
                                    Order = content.Order
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(pages);
        }

        [HttpGet("colors")]
        public async Task<ActionResult<List<Color>>> GetColors()
        {
            var colors = await _context.Colors
                .ToListAsync();

            return Ok(colors);
        }

        [HttpGet("fonts")]
        public async Task<ActionResult<List<Font>>> GetFonts()
        {
            var fonts = await _context.Fonts.Include(f => f.Asset)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Style,
                    f.Weight,
                    Asset = new
                    {
                        f.Asset.Name,
                        f.Asset.FileUrl,
                    }

                }).ToListAsync();
            return Ok(fonts);
        }

        [HttpGet("settings")]
        public async Task<ActionResult> GetSettings()
        {
            var settings = await _settings.GetAsync();

            return Ok(settings);
        }

    }
}