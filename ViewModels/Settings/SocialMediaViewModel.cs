using WebAppBackend.Models;

namespace WebAppBackend.ViewModels.Settings
{
    public class SocialMediaViewModel
    {
        public int Id { get; set; }

        public string? HeaderText { get; set; }
        public bool InstagramVisible { get; set; }
        public string? InstagramUrl { get; set; }


        public bool FacebookVisible { get; set; }
        public string? FacebookUrl { get; set; }


        public bool TwitterVisible { get; set; }
        public string? TwitterUrl { get; set; }

        public object? Summary { get; set; }
    }
}
