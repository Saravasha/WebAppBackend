using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{

    public class ContentViewModel
    {


        public int Id { get; set; }
        [Required]
        [Display(Name = "Content Title")]
        public string Title { get; set; }
        [JsonIgnore]
        public DateOnly? Date { get; set; }
        public string? DateString => Date?.ToString("yyyy-MM-dd");
        [Display(Name = "Container Body")]
        [JsonIgnore]
        public string? Container { get; set; }
        public int Order { get; set; }
        public int? ChapterId { get; set; }
        public Page? Page { get; set; }
        public Chapter? Chapter { get; set; }

        public List<int>? ChapterIds { get; set; } = new();
        public List<Chapter>? Chapters { get; set; } = new();

        public RelationshipViewModel Relationship { get; set; } = new();

        public bool CanMoveUp { get; set; }
        public bool CanMoveDown { get; set; }

    }
}
