using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("QuotationReport", Schema = "Sales")]
    public class QuotationReport
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Quotation Date")]
        public DateTime? QuotationDate { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public int CompanyNameId { get; set; }

        [ForeignKey("CompanyNameId")]
        public virtual CompanyName? Company { get; set; }

        [Display(Name = "Product Details")]
        public string? ProductDetails { get; set; }

        [Display(Name = "Customber Details")]
        public string? CustomerDetails { get; set; }

        [Display(Name = "Value")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Invalid value. Maximum allowed: 18 digits, 2 decimal places.")]
        public decimal? QuotationValue { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Is Positive?")]
        public byte? IsPositive { get; set; }

        [NotMapped]
        public bool IsPositiveBool { get => IsPositive == 1; set => IsPositive = value ? (byte)1 : (byte)0; }

        public DateTime? CreatedDateTime { get; set; }

        [ForeignKey("AspNetUsers")]
        public string? CreatedUserId { get; set; }

        [NotMapped]
        public string? CreatedUserName { get; set; }

        [NotMapped]
        [DisplayName("Company Name")]
        public string? CompanyNameStr { get; set; }
    }
}
