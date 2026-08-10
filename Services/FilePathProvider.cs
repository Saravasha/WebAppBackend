public class FilePathProvider
{
    private readonly IWebHostEnvironment _env;

    public FilePathProvider(IWebHostEnvironment env)
    {
        _env = env;
    }

    //for staging 
    public string RobotsTxtPath => Path.Combine(_env.WebRootPath ?? "wwwroot", "robots.txt");
    public string UploadsRoot => Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.FullName, "Uploads");

    public string PublishRoot => Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.FullName, "Publish");

    public string ThumbnailsRoot => Path.Combine(UploadsRoot, "Thumbnails");

    //public string WebAssetsRoot => Path.Combine(_env.WebRootPath, "Assets");

    public string ToWebPath(string fullPath)
    {
        var uploadsRoot = Path.GetFullPath(UploadsRoot) + Path.DirectorySeparatorChar;
        var publishRoot = Path.GetFullPath(PublishRoot) + Path.DirectorySeparatorChar;
        //var webRoot = Path.GetFullPath(_env.WebRootPath) + Path.DirectorySeparatorChar;

        if (fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = fullPath.Substring(uploadsRoot.Length);
            return "/Uploads/" + relativePath.Replace("\\", "/");
        }

        if (fullPath.StartsWith(publishRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = fullPath.Substring(publishRoot.Length);
            return "/Publish/" + relativePath.Replace("\\", "/");
        }

        //if (fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
        //{
        //    var relativePath = fullPath.Substring(webRoot.Length);
        //    return "/" + relativePath.Replace("\\", "/");
        //}

        // Debug fallback
        return "/unmapped/" + fullPath.Replace("\\", "/");
    }

    public string GetFullPath(string webPath)
    {
        if (string.IsNullOrEmpty(webPath))
            throw new ArgumentException("Path cannot be null or empty", nameof(webPath));

        // Normalize slashes
        webPath = webPath.Replace('\\', '/');

        // Expecting webPath like "/Uploads/filename.ext"
        const string uploadsPrefix = "/Uploads/";
        const string publishPrefix = "/Publish/";
        var fullPath = string.Empty;
        var relativePath = string.Empty;

        if (webPath.StartsWith(uploadsPrefix, StringComparison.OrdinalIgnoreCase))
        {

            // Strip "/Uploads/" prefix to get relative path
            relativePath = webPath.Substring(uploadsPrefix.Length);

            // Combine with UploadsRoot, making sure to use system directory separators
            fullPath = Path.Combine(UploadsRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
        else if (webPath.StartsWith(publishPrefix, StringComparison.OrdinalIgnoreCase))
        {
            relativePath = webPath.Substring(publishPrefix.Length);
            fullPath = Path.Combine(PublishRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
        else
        {
            throw new ArgumentException($"Invalid path: must start with {uploadsPrefix} or {publishPrefix}", nameof(webPath));
        }

        return fullPath;
    }
}
