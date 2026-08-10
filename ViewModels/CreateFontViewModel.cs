using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAppBackend.Models
{
    public class CreateFontViewModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Style { get; set; }

        public int Weight { get; set; }
        [Required]
        public int? AssetId { get; set; }
        public string? AssetName { get; set; } 

    }
}
