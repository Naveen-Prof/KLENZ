using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("ProjectList", Schema = "Sales")]
    public class ProjectList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("FinancialYear")]
        [DisplayName("Financial Year")]
        public int FyYear { get; set; }  // Foreign key

        [DisplayName("Financial Year")]
        public virtual FinancialYear? FinancialYear { get; set; }

        [Display(Name = "Work Order date")]
        public DateTime? WorkOrderDate { get; set; }

        [Required]
        [ForeignKey("Company")]
        [DisplayName("Company Name")]
        public int CompanyNameId { get; set; }  // Foreign key

        public virtual CompanyName? Company { get; set; }

        [Display(Name = "Customber Details")]
        public string? CustomerDetails { get; set; }

        [Display(Name = "Work Details")]
        public string? WorkDetails { get; set; }

        [Display(Name = "Work Order Value")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Invalid value. Maximum allowed: 18 digits, 2 decimal places.")]
        public decimal? WorkOrderValue { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Created Date")]
        public DateTime? CreatedDateTime { get; set; }

        [StringLength(450)]
        [ForeignKey("AspNetUsers")]
        public string? CreatedUserId { get; set; }

        [NotMapped]
        [Display(Name = "Created User")]
        public string? CreatedUserName { get; set; }

        // Navigation properties
        [NotMapped]
        [DisplayName("Financial Year")]
        public string? FinancialYearStr { get; set; } // Directly get FyYear

        [NotMapped]
        [DisplayName("Company Name")]
        public string? CompanyNameStr { get; set; }  // Added set accessor

    }
}
