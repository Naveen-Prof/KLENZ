using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("Companies", Schema = "Services")]
    public class CompanyName
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Sequence No")]
        public int SequenceNo { get; set; } = 1;

        [Required]
        [MaxLength(500)]
        public required string FullName { get; set; }

        [Required]
        [MaxLength(250)]
        public required string ShortName { get; set; }

        [Required]
        public byte IsActive { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        [ForeignKey("CreatedUser")]
        public string? CreatedUserId { get; set; }

        [NotMapped]
        public string? CreatedUserName { get; set; }
    }
}
