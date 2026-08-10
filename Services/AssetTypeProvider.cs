using WebAppBackend.Models;

namespace WebAppBackend.Services
{
    public class AssetTypeProvider
    {
        public AssetType GetAssetType(string input)
        {

            Console.WriteLine($"AssetTypeProvider input: '{input}'");

            if (string.IsNullOrWhiteSpace(input))
                return AssetType.Other;

            input = input.ToLowerInvariant();

            // Handle file extensions
            if (input.StartsWith("."))
            {
                return input switch
                {
                    ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => AssetType.Image,
                    ".mp4" or ".mov" or ".avi" or ".wmv" => AssetType.Video,
                    ".mp3" or ".wav" or ".ogg" => AssetType.Audio,
                    ".pdf" or ".doc" or ".docx" => AssetType.Document,
                    ".txt" or ".md" => AssetType.Text,
                    ".woff2" or ".woff" or ".ttf" or ".otf" => AssetType.Font,
                    _ => AssetType.Other
                };
            }

            // Handle MIME types
            if (input.StartsWith("image/")) return AssetType.Image;
            if (input.StartsWith("video/")) return AssetType.Video;
            if (input.StartsWith("audio/")) return AssetType.Audio;
            if (input == "application/pdf" ||
               input == "application/msword" ||
               input == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                return AssetType.Document;
            if (input.StartsWith("text/")) return AssetType.Text;
            if (input.StartsWith("font/")) return AssetType.Font;

            return AssetType.Other;
        }
        public AssetType GetAssetType(IFormFile file)
        {
            if (file == null)
                return AssetType.Other;

            // Prefer MIME type when it is specific
            var contentType = file.ContentType?.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(contentType) &&
                contentType != "application/octet-stream")
            {
                if (contentType.StartsWith("image/")) return AssetType.Image;
                if (contentType.StartsWith("video/")) return AssetType.Video;
                if (contentType.StartsWith("audio/")) return AssetType.Audio;
                if (contentType.StartsWith("font/")) return AssetType.Font;
                if (contentType.StartsWith("text/")) return AssetType.Text;

                if (contentType == "application/pdf" ||
                    contentType == "application/msword" ||
                    contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    return AssetType.Document;
                }
            }

            // Fall back to extension
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => AssetType.Image,
                ".mp4" or ".mov" or ".avi" or ".wmv" => AssetType.Video,
                ".mp3" or ".wav" or ".ogg" => AssetType.Audio,
                ".pdf" or ".doc" or ".docx" => AssetType.Document,
                ".txt" or ".md" => AssetType.Text,
                ".ttf" or ".otf" or ".woff" or ".woff2" => AssetType.Font,
                _ => AssetType.Other
            };
        }
    }
}
