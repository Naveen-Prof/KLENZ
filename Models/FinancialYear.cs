using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("FinancialYear", Schema = "Klenz")]
    public class FinancialYear
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Financial Year")]
        public required string FyYear { get; set; }

        [Required]
        [Display(Name = "Is Active?")]
        public byte IsActive { get; set; }

        [NotMapped]
        public bool IsActiveBool { get => IsActive == 1; set => IsActive = value ? (byte)1 : (byte)0; }

        public DateTime? CreatedDateTime { get; set; }

        [ForeignKey("AspNetUsers")]
        public string? CreatedUserId { get; set; }
    }
}
