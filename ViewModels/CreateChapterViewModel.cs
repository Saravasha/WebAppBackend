using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{
    public class CreateChapterViewModel
    {

        [Required(ErrorMessage = "Chapter Title is required")]
        [Display(Name = "Chapter Title:")]
        public string Title { get; set; }
        [JsonIgnore]
        public DateOnly? Date { get; set; }
        public string? DateString => Date?.ToString("yyyy-MM-dd");
        [Display(Name = "Container Body")]
        public string? Container { get; set; }
        [Required(ErrorMessage = "Content Page is required")]
        [Display(Name = "Parent Content")]
        public int Order { get; set; }
        public int? PageId { get; set; }
        public List<int>? PageIds { get; set; } = new();
        public List<Page>? Pages { get; set; } = new();
        public List<int>? ContentIds { get; set; } = new();
        public List<Content>? Contents { get; set; } = new List<Content>();
    }
}
