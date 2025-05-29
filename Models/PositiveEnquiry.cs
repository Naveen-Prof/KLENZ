using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("PositiveEnquiry", Schema = "Sales")]
    public class PositiveEnquiry
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Quotation Date")]
        public DateTime? QuotationDate { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public int CompanyNameId { get; set; }

        [Required]
        [ForeignKey("FinancialYear")]
        [DisplayName("Financial Year")]
        public int FyYear { get; set; }  // Foreign key

        [ForeignKey("CompanyNameId")]
        public virtual CompanyName? Company { get; set; }

        [Display(Name = "Product Details")]
        public string? ProductDetails { get; set; }

        [Display(Name = "Customber Details")]
        public string? CustomerDetails { get; set; }

        [Display(Name = "Value")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Invalid value. Maximum allowed: 18 digits, 2 decimal places.")]
        public decimal? QuotationValue { get; set; }

        [Display(Name = "Current Status")]
        public string? CurrentStatus { get; set; }

        [Display(Name = "Created date time")]
        public DateTime? CreatedDateTime { get; set; }

        [ForeignKey("AspNetUsers")]
        public string? CreatedUserId { get; set; }

        [NotMapped]
        [Display(Name = "Created User")]
        public string? CreatedUserName { get; set; }

        [NotMapped]
        [DisplayName("Company Name")]
        public string? CompanyNameStr { get; set; }

        // Navigation properties
        [NotMapped]
        [DisplayName("Financial Year")]
        public string? FinancialYearStr { get; set; } // Directly get FyYear
    }
}
