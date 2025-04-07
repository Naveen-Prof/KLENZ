using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("Consultancy", Schema = "Project")]
    public class Project_Consultancy
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
        public int? GSTTypeId { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Total { get; set; }

        [MaxLength(100)]
        [DisplayName("Work Duration")]
        public string? WorkDuration { get; set; }

        public string? Remarks { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        [MaxLength(450)]
        public string? CreatedUserId { get; set; }

        [ForeignKey("FyYear")]
        [DisplayName("Financial Year")]
        public virtual FinancialYear? FinancialYear { get; set; }

        [ForeignKey("CompanyNameId")]
        [DisplayName("Company Details")]
        public virtual CompanyName? Company { get; set; }

        [ForeignKey("GSTTypeId")]
        [DisplayName("GST")]
        public virtual GSTTypes? GSTType { get; set; }
    }
}
