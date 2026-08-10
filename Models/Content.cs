
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebAppBackend.Models
{
    public class Content
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
        public int? ChapterId { get; set; }
        public Chapter? Chapter { get; set; }

    }
}
