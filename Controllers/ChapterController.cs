using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;
using WebAppBackend.ViewModels;


namespace WebAppBackend.Controllers
{
    [Authorize]
    public class ChapterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHtmlSanitizerService _htmlSanitizer;


        public ChapterController(ApplicationDbContext context, IHtmlSanitizerService htmlSanitizer)
        {
            _context = context;
            _htmlSanitizer = htmlSanitizer;

        }

        private bool NeedsChapterReIndex(List<Chapter> chapters)
        {
            if (!chapters.Any())
            {
                return false;
            }

            var highestOrder = chapters.Max(p => p.Order);

            // Example:
            // 100 chapters normally means highest order should be around 1000.
            // If it exceeds 100x that, something is unusual.
            var expectedMaximum = chapters.Count * 10;

            return highestOrder > expectedMaximum * 100;
        }

        private async Task ReIndexChaptersAsync(int pageId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.PageId == pageId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            var order = 10;

            foreach (var chapter in chapters)
            {
                chapter.Order = order;
                order += 10;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<IActionResult> Index()
        {
            var chapters = await _context.Chapters
                .Include(c => c.Page)
                .Include(c => c.Contents)
                .OrderBy(c => c.PageId)
                .ThenBy(c => c.Order)
                .ToListAsync();


            var model = new List<ChapterViewModel>();

            foreach (var pageGroup in chapters
                .Where(c => c.Page != null)
                .GroupBy(c => c.PageId))
            {
                var orderedChapters = pageGroup
                    .OrderBy(c => c.Order)
                    .ToList();

                for (int i = 0; i < orderedChapters.Count; i++)
                {
                    var chapter = orderedChapters[i];

                    model.Add(new ChapterViewModel
                    {
                        Id = chapter.Id,
                        Title = chapter.Title,
                        Container = chapter.Container,
                        Date = chapter.Date,
                        Order = chapter.Order,

                        Page = chapter.Page,
                        PageId = chapter.Page?.Id,

                        CanMoveUp = i > 0,
                        CanMoveDown = i < orderedChapters.Count - 1,

                        Contents = chapter.Contents ?? new List<Content>(),

                        Relationship = new RelationshipViewModel
                        {
                            ParentLabel = "Page",
                            ParentTitle = chapter.Page?.Title,

                            ChildLabel = "Contents",
                            Children = chapter.Contents?
                                .Select(c => c.Title)
                                .ToList() ?? new List<string>()
                        }
                    });
                }
            }


            // Add unassigned chapters without ordering
            model.AddRange(chapters
                .Where(c => c.Page == null)
                .Select(chapter => new ChapterViewModel
                {
                    Id = chapter.Id,
                    Title = chapter.Title,
                    Container = chapter.Container,
                    Date = chapter.Date,
                    Order = chapter.Order,

                    Page = null,
                    PageId = null,

                    CanMoveUp = false,
                    CanMoveDown = false,

                    Contents = chapter.Contents ?? new List<Content>()
                }));


            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Page)
                .Include(c => c.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null)
            {
                return NotFound();
            }

            var vm = new ChapterViewModel
            {
                Id = chapter.Id,
                Title = chapter.Title,
                Container = chapter.Container,
                Date = chapter.Date,
                Page = chapter.Page,
                Contents = chapter.Contents ?? new List<Content>(),

                Relationship = new RelationshipViewModel
                {
                    ParentLabel = "Page",
                    ParentTitle = chapter.Page?.Title,

                    ChildLabel = "Contents",
                    Children = chapter.Contents?
              .Select(c => c.Title)
              .ToList() ?? new List<string>()
                }
            };

            return View(vm);
        }


        //GET: PageController/Create
        public IActionResult Create()
        {
            CreateChapterViewModel ccvm = new CreateChapterViewModel();
            var pages = _context.Pages;
            var contents = _context.Contents;

            ViewBag.PageList = new SelectList(pages, "Id", "Title");
            ViewBag.ContentList = new MultiSelectList(contents, "Id", "Title");

            return View(ccvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateChapterViewModel chapter)
        {
            if (!ModelState.IsValid || chapter.PageId == 0)
            {
                if (chapter.PageId == 0)
                {
                    ViewBag.PageError = "Page is required";
                }

                ViewBag.PageList = new SelectList(await _context.Pages.ToListAsync(), "Id", "Title", chapter.PageId);
                ViewBag.ContentList = new MultiSelectList(await _context.Contents.ToListAsync(), "Id", "Title", chapter.ContentIds);
                return View(chapter);
            }
            var pageId = chapter.PageId.Value;


            var existingChapters = await _context.Chapters
                .Where(c => c.PageId == chapter.PageId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            if (NeedsChapterReIndex(existingChapters))
            {
                await ReIndexChaptersAsync(pageId);

                existingChapters = await _context.Chapters
                    .Where(c => c.PageId == chapter.PageId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();
            }


            var highestOrder = existingChapters.Any()
                ? existingChapters.Max(c => c.Order)
                : 0;


            //Sanitize the Container chapter before saving to the database
            var sanitizedContainer = _htmlSanitizer.Sanitize(chapter.Container);

            var chapterToAdd = new Chapter
            {
                Title = chapter.Title,
                Date = chapter.Date,
                Container = sanitizedContainer,
                PageId = chapter.PageId,
                Order = highestOrder + 10
            };

            _context.Chapters.Add(chapterToAdd);

            await _context.SaveChangesAsync();


            if (chapter.ContentIds != null && chapter.ContentIds.Any())
            {
                var contents = await _context.Contents
                    .Where(x => chapter.ContentIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var content in contents)
                {
                    content.ChapterId = chapterToAdd.Id;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null)
            {
                return NotFound();
            }

            foreach (var content in chapter.Contents ?? [])
            {
                Console.WriteLine(
                    $"Content ID: {content.Id}, Title: {content.Title}"
                );
            }

            var ccvm = new CreateChapterViewModel
            {
                Title = chapter.Title,
                Container = chapter.Container,
                Date = chapter.Date,
                PageId = chapter.PageId,

                ContentIds = chapter.Contents?
                    .Select(c => c.Id)
                    .ToList() ?? new List<int>()
            };


            ViewBag.PageList = new SelectList(
                await _context.Pages.ToListAsync(),
                "Id",
                "Title",
                ccvm.PageId
            );


            ViewBag.ContentList = new MultiSelectList(
                await _context.Contents.ToListAsync(),
                "Id",
                "Title",
                ccvm.ContentIds
            );

            return View(ccvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateChapterViewModel chapter)
        {
            
            if (!ModelState.IsValid)
            {
                ViewBag.PageList = new SelectList(await _context.Pages.ToListAsync(), "Id", "Title", chapter.PageId);
                ViewBag.ContentList = new MultiSelectList(await _context.Contents.ToListAsync(), "Id", "Title", chapter.ContentIds);
                return View(chapter);
            }

            var chapterToEdit = await _context.Chapters.FindAsync(id);

            if (chapterToEdit == null)
            {
                return NotFound();
            }

            var originalPageId = chapterToEdit.PageId;
            //Sanitize the Container chapter before saving to the database
            var sanitizedContainer = _htmlSanitizer.Sanitize(chapter.Container);

            chapterToEdit.Title = chapter.Title;
            chapterToEdit.Date = chapter.Date;
            chapterToEdit.Container = sanitizedContainer;
            chapterToEdit.PageId = chapter.PageId;

            _context.Chapters.Update(chapterToEdit);

            if (originalPageId != chapter.PageId)
            {
                var pageId = chapter.PageId!.Value;

                var existingChapters = await _context.Chapters
                    .Where(c => c.PageId == pageId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                if (NeedsChapterReIndex(existingChapters))
                {
                    await ReIndexChaptersAsync(pageId);

                    existingChapters = await _context.Chapters
                        .Where(c => c.PageId == pageId)
                        .OrderBy(c => c.Order)
                        .ToListAsync();
                }

                var highestOrder = existingChapters.Any()
                    ? existingChapters.Max(c => c.Order)
                    : 0;

                chapterToEdit.Order = highestOrder + 10;
            }



            if (chapter.ContentIds != null && chapter.ContentIds.Any())
            {
                var contents = await _context.Contents
                    .Where(x => chapter.ContentIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var content in contents)
                {
                    content.ChapterId = chapterToEdit.Id;
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

            var chapter = await _context.Chapters
               .Include(c => c.Page)
               .Include(c => c.Contents)
               .FirstOrDefaultAsync(c => c.Id == id);


            if (chapter == null)
            {
                return NotFound();
            }


            var vm = new ChapterViewModel
            {
                Id = chapter.Id,
                Title = chapter.Title,
                Container = chapter.Container,
                Date = chapter.Date,
                Page = chapter.Page,
                Contents = chapter.Contents ?? new List<Content>(),

                Relationship = new RelationshipViewModel
                {
                    ParentLabel = "Page",
                    ParentTitle = chapter.Page?.Title,

                    ChildLabel = "Contents",
                    Children = chapter.Contents?
                        .Select(c => c.Title)
                        .ToList() ?? new List<string>()
                 }
            };

            return View(vm);
        }

        // POST: AssetController/Delete/5

        // POST: Db/Delete/5
        [HttpPost, ActionName("Delete")]
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, IFormCollection collection)
        {
            if (_context.Chapters == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Chapters' is null.");
            }
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter != null)
            {
                _context.Chapters.Remove(chapter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MoveUp(int id)
        {
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null || chapter.PageId == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var previousChapter = await _context.Chapters
                .Where(c => c.PageId == chapter.PageId &&
                            c.Order < chapter.Order)
                .OrderByDescending(c => c.Order)
                .FirstOrDefaultAsync();


            if (previousChapter == null)
            {
                 return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var currentOrder = chapter.Order;

            chapter.Order = previousChapter.Order;
            previousChapter.Order = currentOrder;


            await _context.SaveChangesAsync();

            return Redirect($"{Url.Action(nameof(Index))}#{id}");
        }

        public async Task<IActionResult> MoveDown(int id)
        {
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null || chapter.PageId == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var nextChapter = await _context.Chapters
                .Where(c => c.PageId == chapter.PageId &&
                            c.Order > chapter.Order)
                .OrderBy(c => c.Order)
                .FirstOrDefaultAsync();


            if (nextChapter == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var currentOrder = chapter.Order;

            chapter.Order = nextChapter.Order;
            nextChapter.Order = currentOrder;


            await _context.SaveChangesAsync();

            return Redirect($"{Url.Action(nameof(Index))}#{id}");
        }
    }
}
