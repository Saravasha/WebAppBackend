using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{
    public class CreatePageViewModel
    {
        
        [Required(ErrorMessage = "Page Title is required")]
        [Display(Name = "Page Title:")]
        public string Title { get; set; }
        [Display(Name = "Page Main Content")]
        public string? Container { get; set; }
        [Display(Name = "Chapter:")]
        public List<int>? ChapterIds { get; set; } = new();
        public List<Chapter>? Chapters { get; set; } = new();
    }
}
