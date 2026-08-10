using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using WebAppBackend.Data;
using WebAppBackend.Models;
using WebAppBackend.Services;
using WebAppBackend.ViewModels;


namespace WebAppBackend.Controllers
{
    [Authorize]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHtmlSanitizerService _htmlSanitizer;

        public ContentController(ApplicationDbContext context, IHtmlSanitizerService htmlSanitizer)
        {
            _context = context;
            _htmlSanitizer = htmlSanitizer;

        }

        private bool NeedsContentReIndex(List<Models.Content> contents)
        {
            if (!contents.Any())
            {
                return false;
            }

            var highestOrder = contents.Max(p => p.Order);

            // Example:
            // 100 contents normally means highest order should be around 1000.
            // If it exceeds 100x that, something is unusual.
            var expectedMaximum = contents.Count * 10;

            return highestOrder > expectedMaximum * 100;
        }

        private async Task ReIndexContentsAsync(int pageId)
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
            var contents = await _context.Contents
                .Include(c => c.Chapter)
                    .ThenInclude(ch => ch.Page)
                .OrderBy(c => c.ChapterId)
                .ThenBy(c => c.Order)
                .ToListAsync();

            var model = new List<ContentViewModel>();

            foreach (var chapterGroup in contents
                .Where(c => c.Chapter != null)
                .GroupBy(c => c.ChapterId))
            {
                var orderedContents = chapterGroup
                    .OrderBy(c => c.Order)
                    .ToList();

                for (int i = 0; i < orderedContents.Count; i++)
                {
                    var content = orderedContents[i];

                    model.Add(new ContentViewModel
                    {
                        Id = content.Id,
                        Title = content.Title,
                        Container = content.Container,
                        Date = content.Date,
                        Order = content.Order,

                        Chapter = content.Chapter,
                        ChapterId = content.Chapter?.Id,
                        Page = content.Chapter?.Page,

                        CanMoveUp = i > 0,
                        CanMoveDown = i < orderedContents.Count - 1,

                        Relationship = new RelationshipViewModel
                        {
                            ParentLabel = "Page",
                            ParentTitle = content.Chapter?.Page?.Title,

                            ChildLabel = "Chapter",
                            Children = content.Chapter != null
                                ? new List<string>
                                {
                            content.Chapter.Title
                                }
                                : new List<string>()
                        }
                    });
                }
            }

            // Orphan contents
            model.AddRange(contents
                .Where(c => c.Chapter == null)
                .Select(content => new ContentViewModel
                {
                    Id = content.Id,
                    Title = content.Title,
                    Container = content.Container,
                    Date = content.Date,
                    Order = content.Order,

                    Chapter = null,
                    ChapterId = null,
                    Page = null,

                    CanMoveUp = false,
                    CanMoveDown = false
                }));

            return View(model);
        }

        // GET: ContentController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var content = await _context.Contents
                .Include(c => c.Chapter)
                    .ThenInclude(ch => ch.Page)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            var vm = new ContentViewModel
            {
                Id = content.Id,
                Title = content.Title,
                Container = content.Container,
                Date = content.Date,
                Chapter = content.Chapter,
                Page = content.Chapter?.Page,

                Relationship = new RelationshipViewModel
                {
                    ParentLabel = "Page",
                    ParentTitle = content.Chapter?.Page?.Title,

                    ChildLabel = "Chapter",
                    Children = content.Chapter != null
                        ? new List<string>
                        {
                            content.Chapter.Title
                        }
                        : new List<string>()
                }
            };

            return View(vm);
        }

        //GET: PageController/Create
        public IActionResult Create()
        {
            CreateContentViewModel ccvm = new CreateContentViewModel();
            var chapters = _context.Chapters;

            ViewBag.ChapterList = new SelectList(chapters, "Id", "Title");

            return View(ccvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContentViewModel content)
        {
            if (!ModelState.IsValid || content.ChapterId == 0)
            {
                if (content.ChapterId == 0)
                {
                    ViewBag.PageError = "Chapter is required";
                }

                ViewBag.ChapterList = new SelectList(
                    await _context.Chapters.ToListAsync(),
                    "Id",
                    "Title",
                    content.ChapterId
                );

                return View(content);
            }


            var chapterId = content.ChapterId.Value;


            var existingContents = await _context.Contents
                .Where(c => c.ChapterId == chapterId)
                .OrderBy(c => c.Order)
                .ToListAsync();


            if (NeedsContentReIndex(existingContents))
            {
                await ReIndexContentsAsync(chapterId);

                existingContents = await _context.Contents
                    .Where(c => c.ChapterId == chapterId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();
            }


            var highestOrder = existingContents.Any()
                ? existingContents.Max(c => c.Order)
                : 0;


            var sanitizedContainer = _htmlSanitizer.Sanitize(content.Container);


            var contentToAdd = new Models.Content
            {
                Title = content.Title,
                Date = content.Date,
                Container = sanitizedContainer,
                ChapterId = content.ChapterId,
                Order = highestOrder + 10
            };


            _context.Contents.Add(contentToAdd);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            CreateContentViewModel ccvm = new CreateContentViewModel();
            var content = _context.Contents
                .Include(c => c.Chapter)
                .FirstOrDefault(p => p.Id == id);

            if (content != null)
            {
                ccvm.Title = content.Title;
                ccvm.Date = content.Date;
                ccvm.Container = content.Container;
                ccvm.ChapterId = content.ChapterId;


                var chapter = _context.Chapters;

                ViewBag.ChapterList = new SelectList(chapter, "Id", "Title");
            }

            return View(ccvm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateContentViewModel content)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.ChapterList = new SelectList(await _context.Chapters.ToListAsync(), "Id", "Title", content.ChapterId);
                return View(content);
            };

            var contentToEdit = await _context.Contents.FindAsync(id);

            if (contentToEdit == null)
            {
                return NotFound();
            }

            var originalChapterId = contentToEdit.ChapterId;
            //Sanitize the Container content before saving to the database
            var sanitizedContainer = _htmlSanitizer.Sanitize(content.Container);

            contentToEdit.Title = content.Title;
            contentToEdit.Date = content.Date;
            contentToEdit.Container = sanitizedContainer;
            contentToEdit.ChapterId = content.ChapterId;


            if (originalChapterId != content.ChapterId)
            {
                var chapterId = content.ChapterId!.Value;

                var existingContents = await _context.Contents
                    .Where(c => c.ChapterId == chapterId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                if (NeedsContentReIndex(existingContents))
                {
                    await ReIndexContentsAsync(chapterId);

                    existingContents = await _context.Contents
                        .Where(c => c.ChapterId == chapterId)
                        .OrderBy(c => c.Order)
                        .ToListAsync();
                }

                var highestOrder = existingContents.Any()
                    ? existingContents.Max(c => c.Order)
                    : 0;

                contentToEdit.Order = highestOrder + 10;
            }
            _context.Contents.Update(contentToEdit);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.Contents
                .Include(c => c.Chapter)
                    .ThenInclude(ch => ch.Page)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            var vm = new ContentViewModel
            {
                Id = content.Id,
                Title = content.Title,
                Container = content.Container,
                Date = content.Date,
                Chapter = content.Chapter,
                Page = content.Chapter?.Page,

                Relationship = new RelationshipViewModel
                {
                    ParentLabel = "Page",
                    ParentTitle = content.Chapter?.Page?.Title,

                    ChildLabel = "Chapter",
                    Children = content.Chapter != null
                        ? new List<string>
                        {
                            content.Chapter.Title
                        }
                        : new List<string>()
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
            if (_context.Contents == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contents' is null.");
            }
            var chapter = await _context.Contents.FindAsync(id);
            if (chapter != null)
            {
                _context.Contents.Remove(chapter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MoveUp(int id)
        {
            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null || content.ChapterId == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var previousChapter = await _context.Contents
                .Where(c => c.ChapterId == content.ChapterId &&
                            c.Order < content.Order)
                .OrderByDescending(c => c.Order)
                .FirstOrDefaultAsync();


            if (previousChapter == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var currentOrder = content.Order;

            content.Order = previousChapter.Order;
            previousChapter.Order = currentOrder;


            await _context.SaveChangesAsync();

            return Redirect($"{Url.Action(nameof(Index))}#{id}");
        }

        public async Task<IActionResult> MoveDown(int id)
        {
            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id);

            if (content == null || content.ChapterId == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var nextContent = await _context.Contents
                .Where(c => c.ChapterId == content.ChapterId &&
                            c.Order > content.Order)
                .OrderBy(c => c.Order)
                .FirstOrDefaultAsync();


            if (nextContent == null)
            {
                return Redirect($"{Url.Action(nameof(Index))}#{id}");
            }


            var currentOrder = content.Order;

            content.Order = nextContent.Order;
            nextContent.Order = currentOrder;


            await _context.SaveChangesAsync();

            return Redirect($"{Url.Action(nameof(Index))}#{id}");
        }
    }
}
