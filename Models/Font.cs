using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAppBackend.Models
{
    public class Font
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Style { get; set; } = string.Empty;

        public int Weight { get; set; }

        public int? AssetId { get; set; }

        public Asset? Asset { get; set; } = null!;


    }
}
