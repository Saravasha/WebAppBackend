using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebAppBackend.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Chapter Title")]
        public string Title { get; set; }
        [JsonIgnore]
        public DateOnly? Date { get; set; }
        public string? DateString => Date?.ToString("yyyy-MM-dd");
        [Display(Name = "Container Body")]
        public string? Container { get; set; }

        public int Order { get; set; }
        public int? PageId { get; set; }
        public Page? Page { get; set; }

        public List<Content>? Contents { get; set; } = new List<Content>();


    }
}
