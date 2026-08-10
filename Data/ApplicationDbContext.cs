using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppBackend.Models;
using WebAppBackend.Models.SettingsModels;

namespace WebAppBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Page> Pages { get; set; } = default!;
        public DbSet<Content> Contents { get; set; } = default!;
        public DbSet<Chapter> Chapters { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Asset> Assets { get; set; } = default!;
        public DbSet<Color> Colors { get; set; } = default!;
        public DbSet<Font> Fonts { get; set; } = default!;
        public DbSet<Settings> Settings { get; set; } = default!;

        public DbSet<Branding> Branding { get; set; } = default!;
        public DbSet<SocialMedia> SocialMedia { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Color>().HasData(
                new Color { Id = 1, Name = "Background Color", StartColor = "#000000", EndColor = "#000000", DarkStartColor = "#000000", DarkEndColor = "#000000" },
                new Color { Id = 2, Name = "Header Text", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#ffffff", DarkEndColor = "#ffffff" },
                new Color { Id = 3, Name = "Navbar Background Color", StartColor = "#ffff6a", EndColor = "#ffffff", DarkStartColor = "#052e16", DarkEndColor = "#ffffff" },
                new Color { Id = 4, Name = "Page Header Text Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#000000", DarkEndColor = "#ffffff" },
                new Color { Id = 5, Name = "Chapter Header Text Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#000000", DarkEndColor = "#ffffff" },
                new Color { Id = 6, Name = "Content Header Text Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#000000", DarkEndColor = "#ffffff" },
                new Color { Id = 7, Name = "Page Body Text Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#000000", DarkEndColor = "#ffffff" },
                new Color { Id = 8, Name = "Chapter Body Text Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#000000", DarkEndColor = "#ffffff" },
                new Color { Id = 9, Name = "Content Body Text Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#000000", DarkEndColor = "#ffffff" },
                new Color { Id = 10, Name = "ScrollToTop Background Color", StartColor = "#052e16", EndColor = "#052e16", DarkStartColor = "#052e16", DarkEndColor = "#052e16" },
                new Color { Id = 11, Name = "Navbar Text Color", StartColor = "#000000", EndColor = "#000000", DarkStartColor = "#ffffff", DarkEndColor = "#ffffff" },
                new Color { Id = 12, Name = "Instagram Background Icon Color", StartColor = "#15803D", EndColor = "#15803D", DarkStartColor = "#15803D", DarkEndColor = "#15803D" },
                new Color { Id = 13, Name = "Facebook Background Icon Color", StartColor = "#15803D", EndColor = "#15803D", DarkStartColor = "#15803D", DarkEndColor = "#15803D" },
                new Color { Id = 14, Name = "Twitter Background Icon Color", StartColor = "#15803D", EndColor = "#15803D", DarkStartColor = "#15803D", DarkEndColor = "#15803D" },
                new Color { Id = 15, Name = "Instagram Fill Icon Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#ffffff", DarkEndColor = "#ffffff" },
                new Color { Id = 16, Name = "Facebook Fill Icon Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#ffffff", DarkEndColor = "#ffffff" },
                new Color { Id = 17, Name = "Twitter Fill Icon Color", StartColor = "#ffffff", EndColor = "#ffffff", DarkStartColor = "#ffffff", DarkEndColor = "#ffffff" },
                new Color { Id = 18, Name = "Social Media Header Text", StartColor = "#15803D", EndColor = "#15803D", DarkStartColor = "#15803D", DarkEndColor = "#15803D" },
                new Color { Id = 19, Name = "Footer Text", StartColor = "#15803D", EndColor = "#15803D", DarkStartColor = "#15803D", DarkEndColor = "#15803D" });


            modelBuilder.Entity<Font>().HasData(
                new Font
                {
                    Id = 1,
                    Name = "Website Title Header Text Font",
                    Style = "normal",
                    Weight = 100
                },
                new Font
                {
                    Id = 2,
                    Name = "Page Header Text Font",
                    Style = "normal",
                    Weight = 100
                } ,
                new Font
                {
                    Id = 3,
                    Name = "Social Media Header Text",
                    Style = "normal",
                    Weight = 100
                },
                new Font
                {
                    Id = 4,
                    Name = "Website Title Footer Text Font",
                    Style = "normal",
                    Weight = 100
                }
            );


            modelBuilder.Entity<Page>().HasData(
                new Page { Id = 1, Title = "Home", Container = @"<p>Home</p>", Order=10 },
                new Page { Id = 2, Title = "Production", Container = @"<p>Production</p>", Order = 20 },
                new Page { Id = 3, Title = "About", Container = @"<p>About</p>", Order = 30 },
                new Page { Id = 4, Title = "Contact", Container = @"<p>Email: <a href='mailto:info@__DOMAIN_NAME__'>info@__DOMAIN_NAME__</a></p>", Order = 40 },
                new Page
                {
                    Id = 5,
                    Title = "Privacy",
                    Container = "",
                    Order = 50
                }
            );

            modelBuilder.Entity<Chapter>().HasData(
                new Chapter { Id = 1, Title = "Welcome", Container = @"<p>Welcome</p>", PageId = 1, Order = 10 },
                new Chapter { Id = 2, Title = "This is what I'm working on", Container = @"<p>Process:</p>",  PageId = 2, Order = 20},
                new Chapter { Id = 3, Title = "Biography", Container = @"<p>Early Life</p>", PageId = 3, Order = 30 },
                new Chapter { Id = 4, Title = "Social Media", Container = @"<p>Faceberrk</p>", PageId = 4 , Order = 40},
                new Chapter { Id = 5, Title = "Cookie Policy", Container = @"<p>We don't use cookies</p>", PageId = 5 , Order = 50},
                new Chapter { Id = 6, Title = "Privacy Policy", Container = @"
            <p>At __DOMAIN_NAME__, we respect your privacy and are committed to protecting your personal data.</p>
            <h3>Cookies</h3>
            <p>Our website does not use cookies to track visitors or personalize content. The only cookies used are for authentication purposes on the backend, which is accessible only to the site owner (administrator) for managing the website. These cookies are essential for secure login and session management and do not affect public visitors.</p>
            <h3>Personal Data</h3>
            <p>We do not collect, track, or share any personal data from visitors. No personal information is gathered through this website.</p>
            <h3>Data Security</h3>
            <p>The backend login area is secured and accessible only by the site owner. We take reasonable measures to protect any stored data related to site administration.</p>
            <h3>Your Rights</h3>
            <p>Since we do not collect personal data from visitors, there are no user data requests applicable. If you have questions or concerns about privacy, please contact us at <a href='mailto:info@__DOMAIN_NAME__'>info@__DOMAIN_NAME__</a></p>", PageId = 5,  Order = 60, Date = DateOnly.FromDateTime(DateTime.Today) }
            );

            modelBuilder.Entity<Content>().HasData(
                new Content { Id = 1, Title = "Policy 1", Container = @"<p>Welcome</p>", ChapterId = 1, Order = 10 },
                new Content { Id = 2, Title = "Policy 2", Container = @"<p>Process:</p>", ChapterId = 1, Order = 20 },
                new Content { Id = 3, Title = "Policy 3", Container = @"<p>Early Life</p>", ChapterId = 1, Order = 30 },
                new Content { Id = 4, Title = "Policy 4", Container = @"<p>Faceberrk</p>", ChapterId = 1, Order = 40},
                new Content { Id = 5, Title = "Policy 5", Container = @"<p>We don't use cookies</p>", ChapterId = 1, Order = 50}
            );



            modelBuilder.Entity<Settings>().HasData(
                new Settings
                {
                    Id = 1
                }
            );

            modelBuilder.Entity<Branding>().HasData(
                new Branding
                {
                    Id = 1,
                    SettingsId = 1,
                    AppName = "__PROJECT_NAME__",
                    Description = "__PROJECT_NAME__ - web application for managing productions, assets, and content."
                }
            );

            modelBuilder.Entity<SocialMedia>().HasData(
                new SocialMedia
                {
                    Id = 1,
                    SettingsId = 1,

                    HeaderText = "Follow me @",

                    InstagramVisible = true,
                    InstagramUrl = "https://www.instagram.com/__INSTAGRAM_USERNAME__",

                    FacebookVisible = true,
                    FacebookUrl = "https://www.facebook.com/__FACEBOOK_USERNAME__",

                    TwitterVisible = true,
                    TwitterUrl = "https://x.com/__TWITTER_USERNAME__"
                }
            );



            modelBuilder.Entity<Branding>(entity =>
            {

                entity.Property(x => x.AppName)
                .IsRequired();

                entity.Property(x => x.Description).HasMaxLength(100);

                entity.HasOne(x => x.LoginImageAsset)
                .WithMany()
                .HasForeignKey(x => x.LoginImageAssetId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.FaviconAsset)
                  .WithMany()
                  .HasForeignKey(x => x.FaviconAssetId)
                  .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Settings>()
                .HasOne(x => x.Branding)
                .WithOne(x => x.Settings)
                .HasForeignKey<Branding>(x => x.SettingsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Settings>()
                .HasOne(x => x.SocialMedia)
                .WithOne(x => x.Settings)
                .HasForeignKey<SocialMedia>(x => x.SettingsId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Page>()
                 .HasMany(x => x.Chapters)
                 .WithOne(x => x.Page)
                 .HasForeignKey(x => x.PageId)
                 .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Chapter>()
                .HasMany(x => x.Contents)
                .WithOne(x => x.Chapter)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Asset>()
               .HasMany(p => p.Categories)
               .WithMany(c => c.Assets);

            modelBuilder.Entity<Asset>()
                .Property(e => e.Date)
                .HasColumnType("date");
            modelBuilder.Entity<Chapter>()
                .Property(e => e.Date)
                .HasColumnType("date");
            modelBuilder.Entity<Content>()
                .Property(e => e.Date)
                .HasColumnType("date");

            modelBuilder.Entity<Font>()
                .HasOne(f => f.Asset)
                .WithMany(a => a.Fonts)
                .HasForeignKey(f => f.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
