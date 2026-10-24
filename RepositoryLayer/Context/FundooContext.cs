
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Context;

public class FundooContext : DbContext // Db context is class provided by EF core that maps our application 
    // entities to our postgres/database tables,columns,
    // a context has generally 4 responsibilities = querying, tracking changes,saving and model configuration
    // FundooContext as the EF Core session through which your repository interacts with the database
{
    public FundooContext(DbContextOptions<FundooContext> options)
        : base(options)
    {
    }

    // DbContextOptions<FundooContext> contains the configuration needed by EF Core for this context,
    // such as the selected database provider and connection configuration.
    //The : base(options) syntax passes those options to the parent DbContext constructor.
    // in program.cs we wrote something like adddbcontext usenpsql

    // A DbSet<TEntity> represents a collection of entities of a particular
    // type that EF Core can query and track for persistence.

    // Db set doesn't change or assign table names, it is done in migration

    public DbSet<UserEntity> Users { get; set; }

    public DbSet<ExternalLoginEntity> ExternalLogins { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    base.OnModelCreating(modelBuilder);

    //    modelBuilder.Entity<ExternalLoginEntity>()
    //        .HasIndex(e => new { e.Provider, e.ProviderSubject })
    //        .IsUnique();

    //    modelBuilder.Entity<ExternalLoginEntity>()
    //        .HasOne(e => e.User)
    //        .WithMany()
    //        .HasForeignKey(e => e.UserId)
    //        .OnDelete(DeleteBehavior.Cascade);
    //}
    public DbSet<NoteEntity> Notes { get; set; }
}


// EF Core uses your entity configuration and database model to determine how the two relate.
//3. So, who maps the entity to the database?
//Entity Framework Core(EF Core) is the main component responsible for translating entity operations into database commands.
//In your Fundoo project, the RepositoryLayer likely uses EF Core through a DbContext.
