using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{

    public class ChapterViewModel
    {


        public int Id { get; set; }
        [Required]
        [Display(Name = "Chapter Title")]
        public string Title { get; set; }
        [JsonIgnore]
        public DateOnly? Date { get; set; }
        public string? DateString => Date?.ToString("yyyy-MM-dd");
        [Display(Name = "Container Body")]
        [JsonIgnore]
        public string? Container { get; set; }
        public int Order { get; set; }
        public int? PageId { get; set; }
        public Page? Page { get; set; }

        public List<int>? PageIds { get; set; } = new();
        public List<Page>? Pages { get; set; } = new();
        public List<Content>? Contents { get; set; } = new();
        public RelationshipViewModel Relationship { get; set; } = new();
        public bool CanMoveUp { get; set; }
        public bool CanMoveDown { get; set; }

    }
}
