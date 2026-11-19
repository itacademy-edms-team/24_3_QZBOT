using Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityUserContext<ApplicationUser>(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<CourtCase> CourtCases => Set<CourtCase>();
    public DbSet<HttpRequestLog> HttpRequestLogs => Set<HttpRequestLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ArchiveUpload>(entity =>
        {
            entity.ToTable("ArchiveUploads");
            entity.Property(upload => upload.Note).HasMaxLength(Document.MaxNoteLength);
        });

        builder.Entity<CourtCase>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(255);
            entity.HasIndex(item => new { item.OwnerId, item.CreatedAt });
            entity.HasOne(item => item.Owner)
                .WithMany()
                .HasForeignKey(item => item.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<HttpRequestLog>(entity =>
        {
            entity.Property(log => log.Method).HasMaxLength(32);
            entity.Property(log => log.RouteTemplate).HasMaxLength(512);
            entity.Property(log => log.TraceId).HasMaxLength(128);
            entity.Property(log => log.UserId).HasMaxLength(450);
            entity.HasIndex(log => log.StartedAtUtc);
        });

        builder.Entity<Document>(entity =>
        {
            entity.Property(document => document.Note).HasMaxLength(Document.MaxNoteLength);
            entity.HasOne(document => document.ArchiveUpload)
                .WithMany()
                .HasForeignKey(document => document.ArchiveUploadId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne(document => document.Case)
                .WithMany(item => item.Documents)
                .HasForeignKey(document => document.CaseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_Documents_SizeBytes", "[SizeBytes] > 0"));
        });
    }
}
