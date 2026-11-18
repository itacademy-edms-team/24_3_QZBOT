using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityUserContext<ApplicationUser>(options)
{
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Document>(entity =>
        {
            entity.HasKey(document => document.Id);
            entity.Property(document => document.OwnerId).IsRequired();
            entity.Property(document => document.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(document => document.StorageKey).HasMaxLength(128).IsRequired();
            entity.Property(document => document.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(document => document.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(document => document.UploadedAt).IsRequired();

            entity.HasOne(document => document.Owner)
                .WithMany()
                .HasForeignKey(document => document.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(document => document.StorageKey).IsUnique();
            entity.HasIndex(document => new { document.OwnerId, document.UploadedAt });
            entity.HasIndex(document => document.CaseId);
            entity.ToTable(table => table.HasCheckConstraint("CK_Documents_SizeBytes", "[SizeBytes] > 0"));
        });
    }
}
