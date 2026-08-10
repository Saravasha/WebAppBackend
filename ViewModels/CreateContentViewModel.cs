using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{
    public class CreateContentViewModel
    {

        [Required(ErrorMessage = "Content Title is required")]
        [Display(Name = "Content Title:")]
        public string Title { get; set; }
        [JsonIgnore]
        public DateOnly? Date { get; set; }
        public string? DateString => Date?.ToString("yyyy-MM-dd");
        [Display(Name = "Container Body")]
        public string? Container { get; set; }
        public int Order { get; set; }
        [Required(ErrorMessage = "Parent Chapter is required")]
        [Display(Name = "Parent Chapter")]
        public int? ChapterId { get; set; }
        public List<int>? ChapterIds { get; set; } = new();
        public List<Chapter>? Chapters { get; set; } = new();
    }
}
