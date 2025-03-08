using System;
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
        [Display(Name = "Financial Year")]
        [ForeignKey("FinancialYear")]
        public required int FyYear { get; set; }

        [Display(Name = "Work Order date")]
        public DateTime? WorkOrderDate { get; set; }

        [StringLength(600)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Customber Details")]
        public string? CustomerDetails { get; set; }

        [Display(Name = "Work Details")]
        public string? WorkDetails { get; set; }

        [Display(Name = "Work Order Value")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Invalid value. Maximum allowed: 18 digits, 2 decimal places.")]
        public decimal? WorkOrderValue { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        [StringLength(450)]
        [ForeignKey("AspNetUsers")]
        public string? CreatedUserId { get; set; }

        // Navigation properties
        public virtual FinancialYear? FinancialYear { get; set; }
    }
}
