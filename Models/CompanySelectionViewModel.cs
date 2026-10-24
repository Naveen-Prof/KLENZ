using System.Collections.Generic;

namespace KLENZ.Models
{
    public class CompanyCardViewModel
    {
        public int Id { get; set; }
        public int SequenceNo { get; set; }
        public required string FullName { get; set; }
        public required string ShortName { get; set; }
        public int SalesCount { get; set; }
        public int ProjectCount { get; set; }
        public int QuotationCount { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class CompanySelectionViewModel
    {
        public List<CompanyCardViewModel> Companies { get; set; } = new();
        public int? CurrentCompanyId { get; set; }
        public string? CurrentCompanyName { get; set; }
    }
}
