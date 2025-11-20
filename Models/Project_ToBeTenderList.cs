using System.ComponentModel;
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
        [DisplayName("Financial Year")]
        public int FyYear { get; set; }

        [DisplayName("Work order date")]
        public DateTime? WorkOrderDate { get; set; }

        [Required]
        [DisplayName("Company Name")]
        public int CompanyNameId { get; set; }

        [DisplayName("Customer")]
        public string? CustomerDetails { get; set; }

        [DisplayName("Work Details")]
        public string? WorkDetails { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayName("Project Cost")]
        public decimal? ProjectCost { get; set; }
        
        [DisplayName("GST")]
        public int? GSTTypeId { get; set; } // Computed column, might be handled separately

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Total { get; set; } // Computed column, might be handled separately

        [MaxLength(100)]
        [DisplayName("Work Duration")]
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
