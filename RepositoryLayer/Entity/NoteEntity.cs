using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepositoryLayer.Entity
{
    public class NoteEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Color { get; set; } = string.Empty;

        public bool IsArchieved { get; set; }

        public bool isDeleted { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public UserEntity User { get; set; }

    }
}
