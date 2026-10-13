
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepositoryLayer.Entity;

public class ExternalLoginEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ExternalLoginId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ProviderSubject { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;
}