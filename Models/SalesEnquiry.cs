using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

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


        [Display(Name = "Refered by")]
        public string? ReferedBy { get; set; }


        [Display(Name = "Enquiry details")]
        public string? EnquiryDetails { get; set; }


        [Display(Name = "Enquiry Date")]
        public DateTime? EnquiryDate { get; set; }


        [Display(Name = "Customer details")]
        public string? CustomerDetails { get; set; }


        public string? Status { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Meeting Date")]
        public DateTime? ReminderDate { get; set; }

        [Display(Name = "Meeting Place")]
        public string? ReminderPlace { get; set; }

        [Display(Name = "Attachment")]
        public string? FilePath { get; set; } // Stores file path in DB

        [NotMapped] // This prevents EF from mapping this property to the DB
        [Display(Name = "Upload File")]
        public IFormFile? File { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public string? CreatedUserId { get; set; }

        [NotMapped]
        public string? CreatedUserName { get; set; }
    }
}
