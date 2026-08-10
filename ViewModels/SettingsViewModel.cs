using WebAppBackend.Models;
using WebAppBackend.ViewModels.Settings;

namespace WebAppBackend.ViewModels
{
    public class SettingsViewModel
    {
        public List<SettingsModuleViewModel> Modules { get; set; } = [];

    }

    public class SettingsModuleViewModel
    {

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public object? Summary { get; set; }

    }
}
