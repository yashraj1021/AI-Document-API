using DocumentQnA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentQnA.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<QnAHistory> QnAHistories => Set<QnAHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            entity.Property(d => d.FileType).IsRequired().HasMaxLength(10);
            entity.Property(d => d.UserId).IsRequired().HasMaxLength(255);
            entity.Property(d => d.Status).HasConversion<string>();

            entity.HasMany(d => d.Chunks)
                  .WithOne(c => c.Document)
                  .HasForeignKey(c => c.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.QnAHistories)
                  .WithOne(h => h.Document)
                  .HasForeignKey(h => h.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Content).IsRequired();
            entity.Property(c => c.VectorId).IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<QnAHistory>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Question).IsRequired();
            entity.Property(h => h.Answer).IsRequired();
            entity.Property(h => h.UserId).IsRequired().HasMaxLength(255);
        });
    }
}