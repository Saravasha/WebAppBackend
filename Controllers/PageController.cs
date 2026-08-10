using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;
using WebAppBackend.ViewModels;


namespace WebAppBackend.Controllers
{
    [Authorize]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHtmlSanitizerService _htmlSanitizer;

        public PageController(ApplicationDbContext context, IHtmlSanitizerService htmlSanitizer)
        {
            _context = context;
            _htmlSanitizer = htmlSanitizer;
        }

        private bool NeedsPageReIndex(List<Page> pages)
        {
            if (!pages.Any())
            {
                return false;
            }

            var highestOrder = pages.Max(p => p.Order);

            // Example:
            // 100 pages normally means highest order should be around 1000.
            // If it exceeds 100x that, something is unusual.
            var expectedMaximum = pages.Count * 10;

            return highestOrder > expectedMaximum * 100;
        }

        private async Task ReIndexPagesAsync()
        {
            var pages = await _context.Pages
                .OrderBy(p => p.Order)
                .ToListAsync();

            var order = 10;

            foreach (var page in pages)
            {
                page.Order = order;
                order += 10;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<IActionResult> Index()
        {
            var pages = await _context.Pages
                .OrderBy(p => p.Order)
                .Include(p => p.Chapters)
                .ThenInclude(c => c.Contents)
                .ToListAsync();

            var firstOrder = pages.FirstOrDefault()?.Order;
            var lastOrder = pages.LastOrDefault()?.Order;
            var pageCount = pages.Count;

            var model = pages.Select((page, index) => new PageViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Container = page.Container,
                Order = page.Order,

                CanMoveUp = pageCount > 1 && index > 0,
                CanMoveDown = pageCount > 1 && index < pageCount - 1,

                Chapters = page.Chapters ?? new List<Chapter>(),

                Relationship = new RelationshipViewModel
                {
                    ChildLabel = "Chapters",
                    Children = page.Chapters?
                        .Select(c => c.Title)
                        .ToList() ?? new List<string>(),

                    GrandchildLabel = "Contents",
                    Grandchildren = page.Chapters?
                        .SelectMany(c => c.Contents ?? new List<Content>())
                        .Select(c => c.Title)
                        .ToList() ?? new List<string>()
                }

            }).ToList();

            return View(model);
        }

        // GET: ContentController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var page = await _context.Pages
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            var vm = new PageViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Container = page.Container,
                Order = page.Order,
                Chapters = page.Chapters ?? new List<Chapter>(),

                Relationship = new RelationshipViewModel
                {
                    ChildLabel = "Chapters",
                    Children = page.Chapters?
                        .Select(c => c.Title)
                        .ToList() ?? new(),

                    GrandchildLabel = "Contents",
                    Grandchildren = page.Chapters?
                        .SelectMany(c => c.Contents ?? new())
                        .Select(c => c.Title)
                        .ToList() ?? new()
                }
            };

            return View(vm);
        }

        //GET: PageController/Create
        public async Task<IActionResult> Create()
        {
            CreatePageViewModel cpvm = new CreatePageViewModel();
            var chapters = _context.Chapters;

            ViewBag.ChapterList = new MultiSelectList(chapters, "Id", "Title");

            return View(cpvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePageViewModel page)
        {

            if (ModelState.IsValid)
            {

                var existingPages = await _context.Pages
                    .OrderBy(p => p.Order)
                    .ToListAsync();

                if (NeedsPageReIndex(existingPages))
                {
                    await ReIndexPagesAsync();

                    existingPages = await _context.Pages
                        .OrderBy(p => p.Order)
                        .ToListAsync();
                }

                var highestOrder = existingPages.Any()
                    ? existingPages.Max(p => p.Order)
                    : 0;

                //Sanitize the Container content before saving to the database
                var sanitizedContainer = _htmlSanitizer.Sanitize(page.Container);

                var pageToAdd = new Page
                {
                    Title = page.Title,
                    Container = sanitizedContainer,
                    Order = highestOrder + 10,
                    Chapters = new List<Chapter>()
                };

                if (page.ChapterIds != null && page.ChapterIds.Any())
                {
                    var selectedChapters = await _context.Chapters
                        .Where(c => page.ChapterIds.Contains(c.Id))
                        .ToListAsync();

                    pageToAdd.Chapters.AddRange(selectedChapters);
                }


                _context.Pages.Add(pageToAdd); ;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Repopulate ChapterList if validation fails
            ViewBag.ChapterList = new MultiSelectList(await _context.Chapters.ToListAsync(), "Id", "Title", page.ChapterIds);

            return View(page);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _context.Pages
                .Include(p => p.Chapters)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            foreach (var chapter in page.Chapters ?? [])
            {
                Console.WriteLine(
                    $"Chapter ID: {chapter.Id}, Title: {chapter.Title}"
                );
            }

            var cpvm = new CreatePageViewModel
            {
                Title = page.Title,
                Container = page.Container,
                ChapterIds = page.Chapters?
                    .Select(c => c.Id)
                    .ToList() ?? new List<int>()
            };

            ViewBag.ChapterList = new MultiSelectList(
                await _context.Chapters.ToListAsync(),
                "Id",
                "Title",
                cpvm.ChapterIds
            );

            return View(cpvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreatePageViewModel page)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ChapterList = new MultiSelectList(await _context.Chapters.ToListAsync(), "Id", "Title", page.ChapterIds);
                return View(page);
            }

            //Sanitize the Container content before saving to the database
            var sanitizedContainer = _htmlSanitizer.Sanitize(page.Container);

            var pageToEdit = await _context.Pages.Include(p => p.Chapters).FirstOrDefaultAsync(p => p.Id == id);

            if (pageToEdit == null)
            {
                return NotFound();
            }

            pageToEdit.Title = page.Title;
            pageToEdit.Container = sanitizedContainer;

            // Clear old contents
            pageToEdit.Chapters.Clear();

            if (page.ChapterIds != null && page.ChapterIds.Any())
            {
                var selectedChapters = await _context.Chapters
                    .Where(c => page.ChapterIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var content in selectedChapters)
                {
                    pageToEdit.Chapters.Add(content);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .Include(p => p.Chapters)
                    .ThenInclude(ch => ch.Contents)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            var vm = new PageViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Container = page.Container,
                Order = page.Order,
                Chapters = page.Chapters?.ToList() ?? new List<Chapter>(),

                Relationship = new RelationshipViewModel
                {
                    ChildLabel = "Chapters",
                    Children = page.Chapters?
                        .Select(c => c.Title)
                        .ToList() ?? new(),

                    GrandchildLabel = "Contents",
                    Grandchildren = page.Chapters?
                        .SelectMany(c => c.Contents ?? new())
                        .Select(c => c.Title)
                        .ToList() ?? new()
                }
            };

            return View(vm);
        }

        // POST: AssetController/Delete/5

        // POST: Db/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.Pages
                .Include(p => p.Chapters).ThenInclude(ch => ch.Contents)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            foreach (var chapter in page.Chapters)
            {
                chapter.PageId = null;
            }

            _context.Pages.Remove(page);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MoveUp(int id)
        {
            var page = await _context.Pages
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }


            var previousPage = await _context.Pages
                .Where(p => p.Order < page.Order)
                .OrderByDescending(p => p.Order)
                .FirstOrDefaultAsync();


            if (previousPage != null)
            {
                var tempOrder = page.Order;

                page.Order = previousPage.Order;
                previousPage.Order = tempOrder;

                await _context.SaveChangesAsync();
            }


            return Redirect($"{Url.Action(nameof(Index))}#{id}");

        }
        public async Task<IActionResult> MoveDown(int id)
        {
            var page = await _context.Pages
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }


            var nextPage = await _context.Pages
                .Where(p => p.Order > page.Order)
                .OrderBy(p => p.Order)
                .FirstOrDefaultAsync();


            if (nextPage != null)
            {
                var tempOrder = page.Order;

                page.Order = nextPage.Order;
                nextPage.Order = tempOrder;

                await _context.SaveChangesAsync();
            }


            return Redirect($"{Url.Action(nameof(Index))}#{id}");
        }
    }
}