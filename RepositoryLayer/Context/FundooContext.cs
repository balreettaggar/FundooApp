
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Context;

public class FundooContext : DbContext
{
    public FundooContext(DbContextOptions<FundooContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }

    public DbSet<ExternalLoginEntity> ExternalLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExternalLoginEntity>()
            .HasIndex(e => new { e.Provider, e.ProviderSubject })
            .IsUnique();

        modelBuilder.Entity<ExternalLoginEntity>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<NoteEntity> Notes { get; set; }
}
