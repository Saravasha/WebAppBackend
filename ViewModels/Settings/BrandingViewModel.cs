using WebAppBackend.Models;

namespace WebAppBackend.ViewModels.Settings
{
    public class BrandingViewModel
    {

        public int Id { get; set; }
        public string AppName { get; set; } = "__PROJECT_NAME__";

        public string? Description { get; set; } = "__PROJECT_NAME__ - web application for managing productions, assets, and content.";


        // Login Background Asset Image
        public int? LoginImageAssetId { get; set; }

        public Asset? LoginImageAsset { get; set; }

        // Favicon

        public int? FaviconAssetId { get; set; }

        public Asset? FaviconAsset { get; set; }

        //Homescreen Image 

        public int? HomescreenAssetId { get; set; }

        public Asset? HomescreenAsset { get; set; }

        //List of Assets

        public List<Asset> AvailableAssets { get; set; } = [];

    }
}
