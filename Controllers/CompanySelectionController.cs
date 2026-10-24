using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLENZ.Data;
using KLENZ.Models;
using KLENZ.Services;

namespace KLENZ.Controllers
{
    public class CompanySelectionController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly ICompanyContext _companyContext;

        public CompanySelectionController(KLENZDbContext context, ICompanyContext companyContext)
        {
            _context = context;
            _companyContext = companyContext;
        }

        // GET: CompanySelection
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var companies = await _context.CompanyName
                .Where(c => c.IsActive == 1)
                .OrderBy(c => c.SequenceNo)
                .ThenBy(c => c.ShortName)
                .ToListAsync();

            // Load counts grouped by CompanyNameId for fast metrics summary
            var salesCounts = await _context.SalesEnquiries
                .GroupBy(s => s.CompanyNameId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CompanyId, g => g.Count);

            var projectCounts = await _context.ProjectList
                .GroupBy(p => p.CompanyNameId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CompanyId, g => g.Count);

            var quotationCounts = await _context.QuotationReport
                .GroupBy(q => q.CompanyNameId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CompanyId, g => g.Count);

            var currentId = _companyContext.CurrentCompanyId;

            var viewModel = new CompanySelectionViewModel
            {
                CurrentCompanyId = currentId,
                CurrentCompanyName = _companyContext.CurrentCompanyName,
                Companies = companies.Select(c => new CompanyCardViewModel
                {
                    Id = c.Id,
                    SequenceNo = c.SequenceNo,
                    FullName = c.FullName,
                    ShortName = c.ShortName,
                    SalesCount = salesCounts.TryGetValue(c.Id, out int sc) ? sc : 0,
                    ProjectCount = projectCounts.TryGetValue(c.Id, out int pc) ? pc : 0,
                    QuotationCount = quotationCounts.TryGetValue(c.Id, out int qc) ? qc : 0,
                    IsCurrent = currentId.HasValue && currentId.Value == c.Id
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: CompanySelection/Explore/5
        public async Task<IActionResult> Explore(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var company = await _context.CompanyName
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive == 1);

            if (company == null)
            {
                TempData["ErrorMessage"] = "Selected company could not be found or is inactive.";
                return RedirectToAction(nameof(Index));
            }

            _companyContext.SetCompany(company.Id, company.FullName, company.ShortName);
            TempData["SuccessMessage"] = $"Now exploring workspace for {company.ShortName} ({company.FullName}).";

            return RedirectToAction("Index", "Home");
        }

        // GET: CompanySelection/Clear
        public IActionResult Clear()
        {
            _companyContext.ClearCompany();
            return RedirectToAction(nameof(Index));
        }
    }
}
