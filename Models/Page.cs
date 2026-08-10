using System.ComponentModel.DataAnnotations;


namespace WebAppBackend.Models
{
    public class Page
    {

        public int Id { get; set; }
        [Required]
        [Display(Name = "Page Title")]
        public string Title { get; set; }
        [Display(Name = "Container Body")]
        public string? Container { get; set; }

        public int Order { get; set; }

        public List<Chapter>? Chapters { get; set; }
    }
}
