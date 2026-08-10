using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAppBackend.Models
{
    public enum AssetType
    {
        Image,
        Video,
        Audio,
        Document,
        Text,
        Font,
        Other
    }
    public class Asset
    {
        [Key]
        public int Id { get; set; } 
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? Location { get; set; }
        [JsonIgnore]
        public DateOnly? Date { get; set; }
        public string? DateString => Date?.ToString("yyyy-MM-dd");
        public string? FileUrl { get; set; } 
		[NotMapped]
        [JsonIgnore]
		public IFormFile? FileUp { get; set; }
        public AssetType Type { get; set; } = AssetType.Other;
        public int CategoryId { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public string? ThumbnailUrl { get; set; }
        public List<Font> Fonts { get; set; } = new();
    }
}
