
using WebAppBackend.Models.SettingsModels;

namespace WebAppBackend.Models
{
    public class Settings
    {
        public int Id { get; set; }
        public Branding Branding { get; set; }

        public SocialMedia SocialMedia { get; set; }

    }
}
