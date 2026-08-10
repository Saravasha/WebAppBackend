using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;
using WebAppBackend.ViewModels;

namespace WebAppBackend.Controllers
{
    [Authorize]
    public class FontController : Controller

    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AssetTypeProvider _assetTypeProvider;
        private readonly FilePathProvider _filePathProvider;

        public FontController(ApplicationDbContext context, IWebHostEnvironment webHost, AssetTypeProvider assetTypeProvider, FilePathProvider filePathProvider)
        {
            _context = context;
            _webHostEnvironment = webHost;
            _assetTypeProvider = assetTypeProvider;
            _filePathProvider = filePathProvider;
        }

        // GET: FontController
        public async Task<IActionResult> Index()
        {
            var fonts = await _context.Fonts
                .Include(f => f.Asset)
                .ToListAsync();

            return View(fonts);
        }

        // GET: FontController/Create
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            CreateFontViewModel font = new CreateFontViewModel();

             ViewBag.FontAssets = new SelectList(_context.Assets
                .Where(a => a.Type == AssetType.Font)
                .OrderBy(a => a.Name),
            "Id",
            "Name");
            return View(font);
        }

        // POST: FontController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateFontViewModel font)
        {

            if (ModelState.IsValid)
            {
                var newFont = new Font
                {
                    Id = font.Id,
                    Name = font.Name,
                    Style = font.Style,
                    Weight = font.Weight,
                    AssetId = font.AssetId,
                };

                _context.Fonts.Add(newFont);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.FontAssets = new SelectList(
           _context.Assets
               .Where(a => a.Type == AssetType.Font)
               .OrderBy(a => a.Name),
           "Id",
           "Name");

            return View(font);
        }

        // GET: FontController/Edit/5

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var font = await _context.Fonts.Include(f => f.Asset).FirstOrDefaultAsync(p => p.Id == id);

            if (font == null)
            {
                return NotFound();
            }


            ViewBag.FontAssets = new SelectList(
           _context.Assets
               .Where(a => a.Type == AssetType.Font)
               .OrderBy(a => a.Name),
           "Id",
           "Name");

            var viewModel = new CreateFontViewModel
            {
                Id = font.Id,
                Name = font.Name,
                Style = font.Style,
                Weight = font.Weight,
                AssetId = font.AssetId,
                AssetName = font.Asset?.Name

            };



            return View(viewModel);
        }


        // POST: FontController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CreateFontViewModel font)
        {
            if (ModelState.IsValid)
            {
                var updatingFont = await _context.Fonts.Include(f => f.Asset).FirstOrDefaultAsync(c => c.Id == font.Id);

                if (updatingFont == null)
                {
                    return NotFound();
                }

                updatingFont.Name = font.Name;
                updatingFont.Style = font.Style;
                updatingFont.Weight = font.Weight;
                updatingFont.AssetId = font.AssetId;

                _context.Fonts.Update(updatingFont);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewBag.FontAssets = new SelectList(
_context.Assets
   .Where(a => a.Type == AssetType.Font)
   .OrderBy(a => a.Name),
"Id",
"Name");

            return View(font);
        }
        

        // GET: FontController/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            var fonts = await _context.Fonts.FindAsync(id);
            var targetFont = await _context.Fonts.FirstOrDefaultAsync(f => f.Id == id);

            if (id == null || _context.Fonts == null || targetFont == null)
            {
                return NotFound();
            }

            return View(fonts);
        }

        // POST: FontController/Delete/5
        // POST: Db/Delete/5
        [HttpPost, ActionName("Delete")]
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Fonts == null)
            {
                return Problem("Entity set 'ApplicationDbConext.Fonts' is null.");
            }
            var fonts = await _context.Fonts.FindAsync(id);
            if (fonts != null)
            {
                _context.Fonts.Remove(fonts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
