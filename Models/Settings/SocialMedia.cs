namespace WebAppBackend.Models.SettingsModels
{
    public class SocialMedia
    {
        public int Id { get; set; }

        public int SettingsId { get; set; }

        public Settings Settings { get; set; } = null!;

        public string? HeaderText { get; set; }

        public bool InstagramVisible { get; set; } = true;
        public string? InstagramUrl { get; set; }


        public bool FacebookVisible { get; set; } = true;
        public string? FacebookUrl { get; set; }


        public bool TwitterVisible { get; set; } = true;
        public string? TwitterUrl { get; set; }
    }
}
