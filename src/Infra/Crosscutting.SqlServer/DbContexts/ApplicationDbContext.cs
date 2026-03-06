using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Crosscutting.SqlServer.DbContexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ─── DbSets ──────────────────────────────────────────────────────────────
    // Add one DbSet<T> per aggregate root.

    /// <summary>Sample aggregate root. Replace/extend with your own entities.</summary>
    public DbSet<Customer> Customers => Set<Customer>();

    // ─── Model Configuration ─────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configure the Customer aggregate root.
        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            // The Email property is a Value Object (record type).
            // EF Core doesn't know how to store it directly, so we map
            // to/from its inner string value using HasConversion.
            entity.Property(c => c.Email)
                  .HasConversion(
                      email => email.Value,               // VO  → column  (string)
                      value => Email.Create(value))       // column → VO    (re-hydrate)
                  .IsRequired()
                  .HasMaxLength(320);                     // RFC 5321 max email length

            entity.Property(c => c.IsActive).IsRequired();
            entity.Property(c => c.CreatedAt).IsRequired();
        });
    }
}