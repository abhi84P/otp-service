using Microsoft.EntityFrameworkCore;
using OtpManagement.Service.Domain;

namespace OtpManagement.Service.Data;

public class OtpDbContext(DbContextOptions<OtpDbContext> options) : DbContext(options)
{
    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    public DbSet<IdentifierState> IdentifierStates => Set<IdentifierState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OtpRequest>(entity =>
        {
            entity.HasKey(o => o.RequestId);
            entity.Property(o => o.Identifier).IsRequired().HasMaxLength(256);
            entity.Property(o => o.CodeHash).IsRequired().HasMaxLength(64);
            entity.HasIndex(o => new { o.Identifier, o.Status });
        });

        modelBuilder.Entity<IdentifierState>(entity =>
        {
            entity.HasKey(s => s.Identifier);
            entity.Property(s => s.Identifier).HasMaxLength(256);
        });
    }
}
