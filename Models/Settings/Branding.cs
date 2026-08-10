namespace WebAppBackend.Models.SettingsModels
{
    public class Branding
    {
        public int Id { get; set; }

        public int SettingsId { get; set; }
        public Settings Settings { get; set; } = null!;

        public string AppName { get; set; } = "__PROJECT_NAME__";

        public string? Description { get; set; } = "__PROJECT_NAME__ - web application for managing productions, assets, and content.";


        // Login Background Asset Image
        public int? LoginImageAssetId { get; set; }

        public Asset? LoginImageAsset { get; set; }

        // Favicon

        public int? FaviconAssetId { get; set; }

        public Asset? FaviconAsset { get; set; }

        //Homescreen Image that shows on the React public facing frontend

        public int? HomescreenAssetId { get; set; }

        public Asset? HomescreenAsset { get; set; }

    }
}
