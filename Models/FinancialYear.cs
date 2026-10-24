using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLENZ.Models
{
    [Table("FinancialYear", Schema = "Services")]
    public class FinancialYear
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1).AddDays(-1);

        [StringLength(250)]
        [Display(Name = "Financial Year")]
        public string? FyYear { get; set; }

        [Required]
        [Display(Name = "Is Active?")]
        public byte IsActive { get; set; }

        [NotMapped]
        public bool IsActiveBool { get => IsActive == 1; set => IsActive = value ? (byte)1 : (byte)0; }

        [Display(Name = "Company")]
        public int? CompanyNameId { get; set; }

        [ForeignKey("CompanyNameId")]
        public virtual CompanyName? Company { get; set; }

        [NotMapped]
        [Display(Name = "Company Name")]
        public string? CompanyNameStr { get; set; }

        [Display(Name = "Created Date Time")]
        public DateTime? CreatedDateTime { get; set; }

        [ForeignKey("AspNetUsers")]
        [Display(Name = "Created User")]
        public string? CreatedUserId { get; set; }

        [NotMapped]
        public string? CreatedUserName { get; set; }
    }
}
