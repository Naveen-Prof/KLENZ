using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("ToBeTenderList", Schema = "Project")]
    public class Project_ToBeTenderList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FyYear { get; set; }

        public DateTime? WorkOrderDate { get; set; }

        [Required]
        public int CompanyNameId { get; set; }

        public string? CustomerDetails { get; set; }

        public string? WorkDetails { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProjectCost { get; set; }

        public int? GSTTypeId { get; set; } // Computed column, might be handled separately

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Total { get; set; } // Computed column, might be handled separately

        [MaxLength(100)]
        public string? WorkDuration { get; set; }

        public string? Remarks { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        [MaxLength(450)]
        public string? CreatedUserId { get; set; }

        [ForeignKey("FyYear")]
        public virtual FinancialYear? FinancialYear { get; set; }

        [ForeignKey("CompanyNameId")]
        public virtual CompanyName? Company { get; set; }

        [ForeignKey("GSTTypeId")]
        public virtual GSTTypes? GSTType { get; set; }

        [NotMapped]
        [Display(Name = "Created User")]
        public string? CreatedUserName { get; set; }

        [NotMapped]
        [Display(Name = "Company Name")]
        public string? CompanyNameStr { get; set; }
    }
}
