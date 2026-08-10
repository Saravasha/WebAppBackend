using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;
using WebAppBackend.ViewModels;

namespace WebAppBackend.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VideoThumbnailProvider _videoThumbnailProvider;
        private readonly AssetTypeProvider _assetTypeProvider;
        private readonly FilePathProvider _filePathProvider;
        private readonly IMemoryCache _cache;
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

        public AssetController(ApplicationDbContext context, VideoThumbnailProvider videoThumbnailProvider, AssetTypeProvider assetTypeProvider, FilePathProvider filePathProvider, IMemoryCache cache)
        {
            _context = context;
            _videoThumbnailProvider = videoThumbnailProvider;
            _assetTypeProvider = assetTypeProvider;
            _filePathProvider = filePathProvider;
            _cache = cache;
        }

        private async Task<(string PhysicalPath, string WebPath)?> UploadedFile(IFormFile? file)
        {
            if (file == null) return null;

            if (!Directory.Exists(_filePathProvider.UploadsRoot))
            {
                Directory.CreateDirectory(_filePathProvider.UploadsRoot);
            }

            string uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
            string physicalFilePath = Path.Combine(_filePathProvider.UploadsRoot, uniqueFileName);

            using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string webPath = _filePathProvider.ToWebPath(physicalFilePath);
            return (physicalFilePath, webPath);
        }

        private async Task FoldersContentGetter(FilePathProvider filePathProvider, List<string> localFolders)
        {
            int numfiles = 0; 
            //CatalogViewModel cvm = new CatalogViewModel();
            foreach (var folder in localFolders)
            {
                //Category catToAdd = new Category();
                var category = _context.Categories.FirstOrDefault(c => c.Name == folder);
                var subFolder = Directory.GetFiles(Path.Combine(filePathProvider.PublishRoot, folder));

                foreach (var file in subFolder)
                {
                    numfiles++;
                    var fileInDirectory = new FileInfo(file).Name;
                    var existingAsset = _context.Assets.FirstOrDefault(a =>
                        a.Name == fileInDirectory &&
                        a.Categories.Any(c => c.Name == folder));
                    if (existingAsset == null)
                    {

                        var fullPath = Path.Combine(filePathProvider.PublishRoot, folder, fileInDirectory);
                        var webPath = filePathProvider.ToWebPath(fullPath);

                        string? thumbPath = null;


                        var assetType = _assetTypeProvider.GetAssetType(Path.GetExtension(fileInDirectory));

                        Models.Asset assetStage = new Models.Asset()
                        {
                            Name = fileInDirectory,
                            Description = folder,
                            Author = User.Identity.Name,
                            FileUrl = webPath,
                            Type = assetType,

                        };

                        var fileInfo = new FileInfo(assetStage.Name);

                        if (fileInfo.Name.EndsWith("_thumb.jpg",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }


                        if (assetType == AssetType.Video)
                        {
                            thumbPath = await _videoThumbnailProvider.GeneratePublishAsync(fullPath);
                            assetStage.ThumbnailUrl = thumbPath;
                        }

                        if (category != null)
                        {
                            assetStage.Categories.Add(category);
                        }
                        _context.Assets.Add(assetStage);
                    }
                }

            }

            if (numfiles == 0)
            {
                TempData["AssetSeedMissing"] = "The Publish folder is empty, please upload files to the Publish folder before using the Seeding method.";
            } else
            {
                TempData["AssetSeedSuccess"] = "Seeding operation into Assets and Categories: Successful!";
            }
            
            await _context.SaveChangesAsync();
        }

        [Authorize(Roles = "Admin")]
        // GET: CatalogController]
        public async Task<IActionResult> Seed()
        {
            List<string> localFolders = new List<string>();
            var publishRoot = Directory.GetDirectories(_filePathProvider.PublishRoot, "*");
            foreach (var categoryFolder in publishRoot)
            {
                var dir = new DirectoryInfo(categoryFolder).Name;
                localFolders.Add(dir);
            }


            foreach (var file in localFolders)
            {

                if (!_context.Categories.Any(x => x.Name == file))
                {
                    _context.Categories.Update(new Category() { Name = file });
                }
                
            }
            
            await _context.SaveChangesAsync();

            await FoldersContentGetter(_filePathProvider, localFolders);           
            
            return RedirectToAction("Index", "Asset");
        }
        [AllowAnonymous]
        [HttpGet("/Asset/Stream/{id}")]
        public async Task<IActionResult> Stream(int id)
        {
            var asset = await _cache.GetOrCreateAsync($"asset_{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _context.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            });
            if (asset == null || string.IsNullOrWhiteSpace(asset.FileUrl))
            {
                return NotFound("Asset or file URL not found.");
            }
            string filePath; try { filePath = _filePathProvider.GetFullPath(asset.FileUrl); }
            catch (Exception ex)
            { // Optional: log exception here
                return BadRequest("Invalid file path.");
            }
            if (!System.IO.File.Exists(filePath)) return NotFound();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, contentType, enableRangeProcessing: true);
        }

        [AllowAnonymous]
        [HttpGet("/Asset/Render/{id:int}")]
        public async Task<IActionResult> Render(int id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
                return NotFound();

            var filePath = _filePathProvider.GetFullPath(asset.FileUrl);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            var content = await System.IO.File.ReadAllTextAsync(filePath);

            return extension switch
            {
                ".md" => Content(
                    Markdig.Markdown.ToHtml(content, Pipeline),
                    "text/html"),

                ".txt" => Content(
                    $"<pre>{System.Net.WebUtility.HtmlEncode(content)}</pre>",
                    "text/html"),

                _ => BadRequest("Unsupported text asset.")
            };
        }

        [AllowAnonymous]
        [HttpGet("/Asset/Download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
                return NotFound();

            var filePath = _filePathProvider.GetFullPath(asset.FileUrl);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileName = Path.GetFileName(filePath);

            return PhysicalFile(
                filePath,
                "application/octet-stream",
                fileName);
        }
        public async Task<IActionResult> Index(string searchString, DateOnly? fromDate, DateOnly? toDate)
        {
            var assets = await _context.Assets
                .AsNoTracking()
                .Include(a => a.Categories)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.ToLower();

                assets = assets.Where(a =>
                    (a.Name?.ToLower().Contains(searchString) ?? false) ||
                    (a.Description?.ToLower().Contains(searchString) ?? false) ||
                    (a.Location?.ToLower().Contains(searchString) ?? false) ||
                    (a.Author?.ToLower().Contains(searchString) ?? false) ||
                    a.Date.HasValue && a.Date.Value.ToString("yyyy-MM-dd").Contains(searchString) ||
                    a.Categories.Any(c => c.Name.ToLower().Contains(searchString))
                ).ToList();
            }

            // Apply date range filtering
            if (fromDate.HasValue)
            {
                assets = assets.Where(a => a.Date.HasValue && a.Date.Value >= fromDate.Value).ToList();
            }

            if (toDate.HasValue)
            {
                assets = assets.Where(a => a.Date.HasValue && a.Date.Value <= toDate.Value).ToList();
            }

            var avm = new AssetViewModel
            {
                Assets = assets,
                FromDate = fromDate,
                ToDate = toDate
            };

            ViewData["CurrentFilter"] = searchString;
            return View(avm);
        }
        // GET: AssetController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .Include(a => a.Categories)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
            {
                return NotFound();
            }

            var davm = new DetailsAssetViewModel
            {
                Asset = asset
            };

            if (asset.Type == AssetType.Text)
            {
                var fullPath = _filePathProvider.GetFullPath(asset.FileUrl);

                if (System.IO.File.Exists(fullPath))
                {
                    davm.TextContent = await System.IO.File.ReadAllTextAsync(fullPath);
                }
            }

            return View(davm);
        }
        // GET: AssetController/Create
        public async Task<IActionResult> Create()
        {
            CreateAssetViewModel avm = new CreateAssetViewModel();
            var categories = await _context.Categories.ToListAsync();

            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");

            return View(avm);
        }
        [RequestFormLimits(MultipartBodyLengthLimit = 400 * 1024 * 1024)]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateAssetViewModel asset, List<string> Categories)
        {
            CreateAssetViewModel cavm = new CreateAssetViewModel();

            var uploadResult = await UploadedFile(asset.FileUp);
            string? physicalFilePath = uploadResult?.PhysicalPath;
            string? webFilePath = uploadResult?.WebPath;

            asset.FileUrl = webFilePath;

            string? thumbPath = null;

            if (asset.FileUp != null && asset.FileUp.ContentType.StartsWith("video/") && physicalFilePath != null)
            {
                thumbPath = await _videoThumbnailProvider.GenerateAsync(physicalFilePath, Path.GetFileName(physicalFilePath));
            }

            if (ModelState.IsValid)
            {
                var assetType = asset.FileUp != null ? _assetTypeProvider.GetAssetType(asset.FileUp) : AssetType.Other;

                var AssetToAdd = new Models.Asset()
                {
                    Name = asset.Name,
                    Description = asset.Description,
                    Author = asset.Author,
                    FileUrl = webFilePath,
                    ThumbnailUrl = thumbPath,
                    Location = asset.Location,
                    Date = asset.Date,
                    Type = assetType
                };

                foreach (var item in Categories)
                {
                    int castItem = int.Parse(item);
                    var catToAdd = _context.Categories.FirstOrDefault(c => c.Id == castItem);
                    if (catToAdd != null)
                        AssetToAdd.Categories.Add(catToAdd);
                }


                await _context.Assets.AddAsync(AssetToAdd);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            if (asset.Categories.Count == 0)
                ViewBag.CategoryError = "Category is Required";

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");

            return View(cavm);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            EditAssetViewModel eavm = new EditAssetViewModel();
            Models.Asset? asset = await _context.Assets
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (asset == null)
            {
                return NotFound();
            }

            List<int> categoriesIds = asset.Categories.Select(c => c.Id).ToList();

            eavm.Name = asset.Name;
            eavm.Description = asset.Description;
            eavm.FileUrl = asset.FileUrl;
            eavm.Author = asset.Author;
            eavm.CategoryIds = categoriesIds;
            eavm.Location = asset.Location;
            eavm.Date = asset.Date;
            eavm.Type = asset.Type;  // <--- Set the AssetType here

            var categories = _context.Categories;
            ViewBag.CategoryList = new MultiSelectList(categories, "Id", "Name");

            if (asset.Type == AssetType.Text)
            {
                var fullPath = _filePathProvider.GetFullPath(asset.FileUrl);

                if (System.IO.File.Exists(fullPath))
                {
                    eavm.TextContent = await System.IO.File.ReadAllTextAsync(fullPath);
                }
            }

            return View(eavm);
        }

        [RequestFormLimits(MultipartBodyLengthLimit = 400 * 1024 * 1024)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAssetViewModel asset, string ExistingFileUrl)
        {
            var assetToEdit = _context.Assets
                .Include(a => a.Categories)
                .FirstOrDefault(a => a.Id == asset.Id);

            if (assetToEdit == null)
            {
                return NotFound();
            }

            ModelState.Remove("FileUp");

            if (ModelState.IsValid)
            {
                assetToEdit.Name = asset.Name;
                assetToEdit.Description = asset.Description;
                assetToEdit.Author = asset.Author;
                assetToEdit.Location = asset.Location;
                assetToEdit.Date = asset.Date;

                if (asset.FileUp != null)
                {
                    var uploadResult = await UploadedFile(asset.FileUp);
                    string? physicalFilePath = uploadResult?.PhysicalPath;
                    string? webFilePath = uploadResult?.WebPath;

                    assetToEdit.FileUrl = webFilePath;
                    assetToEdit.Type = _assetTypeProvider.GetAssetType(asset.FileUp);

                    string? thumbPath = null;

                    if (asset.FileUp.ContentType.StartsWith("video/") && physicalFilePath != null)
                    {
                        thumbPath = await _videoThumbnailProvider.GenerateAsync(physicalFilePath, Path.GetFileName(physicalFilePath));
                    }

                    assetToEdit.ThumbnailUrl = thumbPath;
                }
                else
                {
                    assetToEdit.FileUrl = ExistingFileUrl;
                }

                assetToEdit.Categories.Clear();
                if (asset.CategoryIds != null)
                {
                    foreach (var categoryId in asset.CategoryIds)
                    {
                        var category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                        {
                            assetToEdit.Categories.Add(category);
                        }
                    }
                }

                _context.Assets.Update(assetToEdit);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();

            ViewBag.CategoryList = new MultiSelectList(categories, "Id", "Name", asset.CategoryIds);

            return View(asset);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Assets == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .AsNoTracking()
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (asset == null)
            {
                return NotFound();
            }
            var isReferencedByFont = await _context.Fonts
                .AnyAsync(f => f.AssetId == id);
            
            var davm = new DeleteAssetViewModel
            {
                Asset = asset,
                IsReferencedByFont = isReferencedByFont
            };
            if (asset.Type == AssetType.Text)
            {
                var fullPath = _filePathProvider.GetFullPath(asset.FileUrl);

                if (System.IO.File.Exists(fullPath))
                {
                    davm.TextContent = await System.IO.File.ReadAllTextAsync(fullPath);
                }
            }

            return View(davm);
        }

        // POST: AssetController/Delete/5

        // POST: Db/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, IFormCollection collection)
        {

            var isReferencedByFont = await _context.Fonts
                .AnyAsync(f => f.AssetId == id);

            if (isReferencedByFont)
            {
                TempData["Error"] =
                    "This asset cannot be deleted because it is used by a font.";

                return RedirectToAction(nameof(Delete), new { id });
            }

            if (_context.Assets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Assets' is null.");
            }
            var cat = await _context.Assets.FindAsync(id);
            if (cat != null)
            {
                _context.Assets.Remove(cat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreateMultipleAssets()
        {
            var vm = new CreateMultipleAssetsViewModel();
            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(vm);
        }

        [RequestFormLimits(MultipartBodyLengthLimit = 400 * 1024 * 1024)]
        [HttpPost]
        public async Task<IActionResult> CreateMultipleAssets(CreateMultipleAssetsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
                return View(model);
            }

            string uploadsFolder = _filePathProvider.UploadsRoot;

            foreach (var file in model.FileUp!)
            {
                var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);
                var physicalFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(physicalFilePath, FileMode.Create);
                await file.CopyToAsync(stream);

                string? thumbPath = null;

                if (file.ContentType.StartsWith("video/"))
                {
                    thumbPath = await _videoThumbnailProvider.GenerateAsync(physicalFilePath, uniqueFileName);
                }

                var asset = new Models.Asset
                {
                    Name = uniqueFileName,
                    Description = model.Description,
                    Author = model.Author,
                    FileUrl = _filePathProvider.ToWebPath(physicalFilePath),
                    ThumbnailUrl = thumbPath,
                    Location = model.Location,
                    Date = model.Date,
                    Type = _assetTypeProvider.GetAssetType(file)
                };

                foreach (var catId in model.Categories)
                {
                    if (int.TryParse(catId, out var id))
                    {
                        var category = _context.Categories.FirstOrDefault(c => c.Id == id);
                        if (category != null)
                            asset.Categories.Add(category);
                    }
                }

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }

}