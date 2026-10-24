using KLENZ.Data;
using KLENZ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KLENZ.Controllers
{
    public class PositiveEnquiriesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly KLENZ.Services.ICompanyContext _companyContext;

        public PositiveEnquiriesController(KLENZDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, KLENZ.Services.ICompanyContext companyContext)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _companyContext = companyContext;
        }

        public string GetActiveFinancialYear(int? companyId = null)
        {
            int compId = companyId ?? _companyContext.CurrentCompanyId ?? 0;
            string financialYear = "";
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'KLENZDbContext' is not configured.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 1 FyYear FROM Services.FinancialYear WHERE IsActive = 1 AND (CompanyNameId = @CompanyId OR (CompanyNameId IS NULL AND @CompanyId = 0)) ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", compId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            financialYear = $"{reader.GetString(0)}";
                        }
                    }
                }
            }
            return financialYear;
        }

        public int GetFyId(int? companyId = null)
        {
            int compId = companyId ?? _companyContext.CurrentCompanyId ?? 0;
            int fyId = 0;
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 1 Id FROM Services.FinancialYear WHERE IsActive = 1 AND (CompanyNameId = @CompanyId OR (CompanyNameId IS NULL AND @CompanyId = 0)) ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", compId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fyId = reader.GetInt32(0);
                        }
                    }
                }
            }

            return fyId;
        }

        // GET: PositiveEnquiries
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var companyNameList = await _context.PositiveEnquiry
                .Where(pe => pe.CompanyNameId == compId)
                .Include(c => c.Company)
                .OrderByDescending(c => c.CreatedDateTime)
                .Select(pe => new PositiveEnquiry
                {
                    Id = pe.Id,
                    QuotationDate = pe.QuotationDate,
                    CompanyNameId = pe.CompanyNameId,
                    FyYear = pe.FyYear,
                    Company = pe.Company,
                    ProductDetails = pe.ProductDetails,
                    CustomerDetails = pe.CustomerDetails,
                    QuotationValue = pe.QuotationValue,
                    CurrentStatus = pe.CurrentStatus,
                    CreatedDateTime = pe.CreatedDateTime,
                    CreatedUserId = pe.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == pe.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System"
                })
                .ToListAsync();

            return View(companyNameList ?? new List<PositiveEnquiry>());
        }

        // GET: PositiveEnquiries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var positiveEnquiry = await _context.PositiveEnquiry
                .Where(m => m.Id == id && m.CompanyNameId == compId)
                .Select(pe => new
                {
                    pe.Id,
                    pe.QuotationDate,
                    pe.ProductDetails,
                    pe.CustomerDetails,
                    pe.QuotationValue,
                    pe.CurrentStatus,
                    pe.CreatedDateTime,
                    pe.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == pe.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    CompanyName = _context.CompanyName
                        .Where(c => c.Id == pe.CompanyNameId)
                        .Select(c => c.ShortName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (positiveEnquiry == null) return NotFound();

            var viewModel = new PositiveEnquiry
            {
                Id = positiveEnquiry.Id,
                QuotationDate = positiveEnquiry.QuotationDate,
                ProductDetails = positiveEnquiry.ProductDetails,
                CustomerDetails = positiveEnquiry.CustomerDetails,
                QuotationValue = positiveEnquiry.QuotationValue,
                CurrentStatus = positiveEnquiry.CurrentStatus,
                CreatedDateTime = positiveEnquiry.CreatedDateTime,
                CreatedUserId = positiveEnquiry.CreatedUserId,
                CreatedUserName = positiveEnquiry.CreatedUserName,
                CompanyNameStr = positiveEnquiry.CompanyName
            };

            return View(viewModel);
        }


        // GET: PositiveEnquiries/Create
        public IActionResult Create()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            ViewBag.ActiveFinancialYear = GetActiveFinancialYear(compId);
            return View();
        }

        // POST: PositiveEnquiries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuotationDate,CompanyNameId,ProductDetails,CustomerDetails,QuotationValue," +
            "CurrentStatus,CreatedDateTime,CreatedUserId")] PositiveEnquiry positiveEnquiry)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            positiveEnquiry.CompanyNameId = compId;

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a positive enquiry.");
                    return View(positiveEnquiry);
                }
                positiveEnquiry.CreatedUserId = userId;
                positiveEnquiry.CreatedDateTime = DateTime.Now;
                positiveEnquiry.FyYear = GetFyId(compId);

                _context.Add(positiveEnquiry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Positive Enquiry created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            ViewBag.ActiveFinancialYear = GetActiveFinancialYear(compId);
            return View(positiveEnquiry);
        }

        // GET: PositiveEnquiries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var positiveEnquiry = await _context.PositiveEnquiry.FirstOrDefaultAsync(pe => pe.Id == id && pe.CompanyNameId == compId);
            if (positiveEnquiry == null) return NotFound();

            ViewBag.Companies = _context.CompanyName
                .Where(c => c.Id == compId || c.IsActive == 1)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
                .ToList();

            return View(positiveEnquiry);
        }

        // POST: PositiveEnquiries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationDate,CompanyNameId,ProductDetails,CustomerDetails" +
            ",QuotationValue,CurrentStatus,CreatedDateTime,CreatedUserId")] PositiveEnquiry positiveEnquiry)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            positiveEnquiry.CompanyNameId = compId;

            if (id != positiveEnquiry.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPosEnquiry = await _context.PositiveEnquiry.FirstOrDefaultAsync(pe => pe.Id == id && pe.CompanyNameId == compId);
                    if (existingPosEnquiry == null) return NotFound();

                    positiveEnquiry.CreatedUserId = existingPosEnquiry.CreatedUserId;
                    positiveEnquiry.CreatedDateTime = existingPosEnquiry.CreatedDateTime;
                    positiveEnquiry.FyYear = existingPosEnquiry.FyYear != 0 ? existingPosEnquiry.FyYear : GetFyId(compId);

                    _context.Entry(existingPosEnquiry).CurrentValues.SetValues(positiveEnquiry);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Positive Enquiry updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositiveEnquiryExists(positiveEnquiry.Id, compId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Companies = _context.CompanyName
               .Where(c => c.Id == compId || c.IsActive == 1)
               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
               .ToList();

            return View(positiveEnquiry);
        }

        // GET: PositiveEnquiries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var positiveEnquiry = await _context.PositiveEnquiry
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);
            if (positiveEnquiry == null) return NotFound();

            return View(positiveEnquiry);
        }

        // POST: PositiveEnquiries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var positiveEnquiry = await _context.PositiveEnquiry.FirstOrDefaultAsync(pe => pe.Id == id && pe.CompanyNameId == compId);
            if (positiveEnquiry != null)
            {
                _context.PositiveEnquiry.Remove(positiveEnquiry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Positive Enquiry deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PositiveEnquiryExists(int id, int companyId)
        {
            return _context.PositiveEnquiry.Any(e => e.Id == id && e.CompanyNameId == companyId);
        }
    }
}
