using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("SalesEnquiry", Schema = "Sales")]
    public class SalesEnquiry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public required string CompanyName { get; set; }

        [Required]
        [Display(Name = "Refered by")]
        public required string ReferedBy { get; set; }

        [Required]
        [Display(Name = "Enquiry details")]
        public required string EnquiryDetails { get; set; }

        [Required]
        [Display(Name = "Enquiry Date")]
        public required DateTime? EnquiryDate { get; set; } 

        [Required]
        [Display(Name = "Customer details")]
        public required string CustomerDetails { get; set; }

        [Required]
        public required string Status { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Remainder / Meeting Date")]
        public DateTime? RemainderDate { get; set; }

        [Display(Name = "Meeting Place")]
        public string? RemainderPlace { get; set; }

        [Display(Name = "Attachment")]
        public string? FilePath { get; set; } // Stores file path in DB

        [NotMapped] // This prevents EF from mapping this property to the DB
        [Display(Name = "Upload File")]
        public IFormFile? File { get; set; }
    }
}
