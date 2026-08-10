using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;

namespace WebAppBackend.Controllers
{

    [Authorize]
    public class PageContentController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly VideoThumbnailProvider _videoThumbnailProvider;
        private readonly AssetTypeProvider _assetTypeProvider;


        public PageContentController(ApplicationDbContext context, VideoThumbnailProvider videoThumbnailProvider,AssetTypeProvider assetTypeProvider)
        {
            _context = context;
            _videoThumbnailProvider = videoThumbnailProvider;
            _assetTypeProvider = assetTypeProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetFonts()
        {
            var fonts = await _context.Assets
                .Where(a => a.Type == AssetType.Font)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.FileUrl
                })
                .ToListAsync();

            return Ok(fonts);
        }

        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _context.Assets.Select(a => new { a.Id, a.FileUrl, a.Name, a.ThumbnailUrl }).ToListAsync();
            return Ok(assets);
        }

        [RequestFormLimits(MultipartBodyLengthLimit = 400 * 1024 * 1024)]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {

            string _uploadsFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).ToString(), "Uploads");
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
            if (file != null && file.Length > 0)
            {

                var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(_uploadsFolder, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                var fileUrl = $"/Uploads/{fileName}";
                var absoluteUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{fileUrl}";
                string? thumbPath = null;
                string? physicalFilePath = filePath;

                var assetType = _assetTypeProvider.GetAssetType(Path.GetExtension(file.FileName));

                var asset = new Asset
                {
                    Name = file.FileName,
                    FileUrl = fileUrl,
                    Author = User.Identity.Name,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Type = assetType,

                };

                if (file.ContentType.StartsWith("video/") && physicalFilePath != null)
                {
                    thumbPath = await _videoThumbnailProvider.GenerateAsync(
                        physicalFilePath,
                        Path.GetFileName(physicalFilePath)
                    );

                    asset.ThumbnailUrl = thumbPath;
                }

                // Get or create the "Uploads" category
                var uploadsCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == "Uploads");

                if (uploadsCategory == null)
                {
                    uploadsCategory = new Category
                    {
                        Name = "Uploads"
                    };

                    _context.Categories.Add(uploadsCategory);
                }

                // Associate category with asset
                asset.Categories.Add(uploadsCategory);

                // Add asset
                _context.Assets.Add(asset);

                // Persist everything
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    id = asset.Id,
                    name = asset.Name,
                    url = absoluteUrl,
                    thumbnailUrl = asset.ThumbnailUrl
                });
            }
                   
            return Json(new { url = "" });
        }
    }
}
