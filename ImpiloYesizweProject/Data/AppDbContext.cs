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

        public DbSet<GalleryImage> GalleryImages { get; set; }

        public DbSet<Donation> Donations { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AdminEmailVerification> AdminEmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminEmailVerification>(entity =>
            {
                entity.HasIndex(v => v.ChallengeId)
                    .IsUnique();

                entity.HasIndex(v => new
                {
                    v.AdminUserId,
                    v.CreatedAtUtc
                });

                entity.HasOne(v => v.AdminUser)
                    .WithMany()
                    .HasForeignKey(v => v.AdminUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Donation>()
                .Property(d => d.Amount)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }

}
