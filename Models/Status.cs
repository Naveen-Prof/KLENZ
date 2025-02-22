using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("Status", Schema = "Sales")]
    public class Status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Full Name")]
        public required string FullName { get; set; }

        [Required]
        [DisplayName("Short Name")]
        public required string ShortName { get; set; }

    }
}
