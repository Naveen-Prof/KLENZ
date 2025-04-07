using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KLENZ.Models;
using Microsoft.AspNetCore.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace KLENZ.Models
{
    [Table("SalesEnquiry", Schema = "Sales")]
    public class SalesEnquiry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public int CompanyNameId { get; set; }

        [ForeignKey("CompanyNameId")]
        public virtual CompanyName? Company { get; set; }

        [Display(Name = "Referred by")]
        public string? ReferedBy { get; set; }

        [Display(Name = "Enquiry details")]
        public string? EnquiryDetails { get; set; }

        [Display(Name = "Enquiry Date")]
        public DateTime? EnquiryDate { get; set; }

        [Display(Name = "Customer details")]
        public string? CustomerDetails { get; set; }

        public string? Status { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Reminder Date")]
        public DateTime? ReminderDate { get; set; }

        [Display(Name = "Reminder Place")]
        public string? ReminderPlace { get; set; }

        [Display(Name = "Attachment")]
        public string? FilePath { get; set; } // Stores file path in DB

        [NotMapped] // This prevents EF from mapping this property to the DB
        [Display(Name = "Upload File")]
        public IFormFile? File { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public string? CreatedUserId { get; set; }

        [NotMapped]
        public string? CreatedUserName { get; set; }

        [NotMapped]
        [DisplayName("Company Name")]
        public string? CompanyNameStr { get; set; }
    }

}
