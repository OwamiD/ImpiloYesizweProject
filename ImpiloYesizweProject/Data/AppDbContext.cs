using Microsoft.EntityFrameworkCore;
using ImpiloYesizweProject.Models;

namespace ImpiloYesizweProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<GalleryImage> GalleryImages { get; set; }

        public DbSet<Donation> Donations { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
    }
}