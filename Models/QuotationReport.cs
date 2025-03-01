using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("QuotationReport", Schema = "Sales")]
    public class QuotationReport
    {
        [Key]
        public int Id { get; set; }

        public DateTime? QuotationDate { get; set; }

        [Required]
        public string? CompanyName { get; set; }

        public string? ProductDetails { get; set; }

        public string? CustomerDetails { get; set; }

        public decimal? QuotationValue { get; set; }

        public string? Remarks { get; set; }

        public byte? IsPositive { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        [ForeignKey("AspNetUsers")]
        public string? CreatedUserId { get; set; }
    }
}
