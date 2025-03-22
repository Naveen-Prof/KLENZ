using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("GSTTypes", Schema = "Services")]
    public class GSTTypes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "GST Type")]
        public int GSTType { get; set; }
        public DateTime? CreatedDateTime { get; set; }

        public string? CreatedUserId { get; set; }

        [NotMapped]
        public string? CreatedUserName { get; set; }    
    }
}
