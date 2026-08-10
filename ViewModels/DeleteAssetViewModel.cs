using WebAppBackend.Models;

namespace WebAppBackend.ViewModels
{
    public class DeleteAssetViewModel
    {
        public Asset Asset { get; set; } = null!;
        public string? TextContent {get; set; }
        public bool IsReferencedByFont { get; set; }
    }
}
