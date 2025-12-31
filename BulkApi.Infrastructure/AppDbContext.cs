using BulkApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BulkApi.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Receipt>(e =>
        {
            e.ToTable("receipts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
        });

        mb.Entity<IdempotencyRecord>(e =>
        {
            e.ToTable("idempotency_records");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(128).IsRequired();
            e.Property(x => x.ResponseJson).HasColumnType("longtext").IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.HasIndex(x => x.Key).IsUnique(); // 冪等性の要
        });
    }
}
