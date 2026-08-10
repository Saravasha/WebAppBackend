using System.ComponentModel.DataAnnotations;
using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{
    public class PageViewModel
    {

        public int Id { get; set; }
        [Display(Name = "Page Title")]
        public string? Title { get; set; }
        [Display(Name = "Page Container Body")]
        public string? Container { get; set; }
        public int Order { get; set; }
        public Chapter? Chapter { get; set; }
        [Display(Name = "Contents")]
        public List<int>? ChapterIds { get; set; } = new();
        public List<Chapter>? Chapters { get; set; } = new();
        public List<Page>? Pages { get; set; } = new();

        public RelationshipViewModel Relationship { get; set; } = new();

        public bool CanMoveUp { get; set; }
        public bool CanMoveDown { get; set; }

    }
}
