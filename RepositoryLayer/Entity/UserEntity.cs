using Microsoft.AspNetCore.Http.HttpResults;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Channels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RepositoryLayer.Entity;

public class UserEntity
{
    //An entity is a C# class that EF Core maps to a database entity type,
    //usually a table. An instance of the class represents one record's data.
    // It describes the structure of a user record that EF Core manages.
    // It is not itself the PostgreSQL table, and it is not quite the same thing as a database schema.

    [Key]
    [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
    // This configures the key as database-generated using the identity mechanism supported by the database provider.
    //For PostgreSQL, the actual identity-column configuration depends on your EF Core provider and migration schema. 
    //Normally, you don't need to supply UserId when registering a new user. The database generates it when the row is inserted, and EF Core can populate the generated value back into the tracked entity.
    public int UserId { get; set; }
    [Required] 
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Salt {  get; set; } = string.Empty;
    
}

//Migrations come into play when you want to create or update the database schema based on
//changes to your C# entity models and FundooContext.
// EF Core needs a corresponding database structure to store these properties. Migrations
// help create that structure and keep it synchronized with changes to your entity model.