using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NoteApplication.Models.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string? name { get; set; }

        public required string email { get; set; }

        public required string password { get; set; }

        public DateTime? created_at { get; set; } = default(DateTime?);

        public DateTime? updated_at { get; set; }

        public ICollection<Note>? Notes { get; set; }
    }
}
