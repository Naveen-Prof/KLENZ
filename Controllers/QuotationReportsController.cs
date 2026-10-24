using KLENZ.Data;
using KLENZ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KLENZ.Controllers
{
    public class QuotationReportsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly KLENZ.Services.ICompanyContext _companyContext;

        public QuotationReportsController(KLENZDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, KLENZ.Services.ICompanyContext companyContext)
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

        // GET: QuotationReports
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var companyNameList = await _context.QuotationReport
                .Where(qr => qr.CompanyNameId == compId)
                .Include(c => c.Company)
                .OrderByDescending(c => c.CreatedDateTime)
                .Select(qr => new QuotationReport
                {
                    Id = qr.Id,
                    QuotationDate = qr.QuotationDate,
                    CompanyNameId = qr.CompanyNameId,
                    FyYear = qr.FyYear,
                    Company = qr.Company,
                    ProductDetails = qr.ProductDetails,
                    CustomerDetails = qr.CustomerDetails,
                    QuotationValue = qr.QuotationValue,
                    Remarks = qr.Remarks,
                    IsPositive = qr.IsPositive,
                    CreatedDateTime = qr.CreatedDateTime,
                    CreatedUserId = qr.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == qr.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System"
                })
                .ToListAsync();

            return View(companyNameList ?? new List<QuotationReport>());
        }

        // GET: QuotationReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var quotationReport = await _context.QuotationReport
                .Where(m => m.Id == id && m.CompanyNameId == compId)
                .Select(qr => new 
                {
                    qr.Id,
                    qr.QuotationDate,
                    qr.ProductDetails,
                    qr.CustomerDetails,
                    qr.QuotationValue,
                    qr.Remarks,
                    qr.IsPositive,
                    qr.CreatedDateTime,
                    qr.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == qr.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    CompanyName = _context.CompanyName
                        .Where(c => c.Id == qr.CompanyNameId)
                        .Select(c => c.ShortName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (quotationReport == null) return NotFound();

            var viewModel = new QuotationReport
            {
                Id = quotationReport.Id,
                QuotationDate = quotationReport.QuotationDate,
                ProductDetails = quotationReport.ProductDetails,
                CustomerDetails = quotationReport.CustomerDetails,
                QuotationValue = quotationReport.QuotationValue,
                Remarks = quotationReport.Remarks,
                IsPositive = quotationReport.IsPositive,
                CreatedDateTime = quotationReport.CreatedDateTime,
                CreatedUserId = quotationReport.CreatedUserId,
                CreatedUserName = quotationReport.CreatedUserName,
                CompanyNameStr = quotationReport.CompanyName
            };

            return View(viewModel);
        }


        // GET: QuotationReports/Create
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

        // POST: QuotationReports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuotationDate,CompanyNameId,ProductDetails,CustomerDetails,QuotationValue,Remarks,IsPositive,CreatedDateTime,CreatedUserId")] QuotationReport quotationReport)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            quotationReport.CompanyNameId = compId;

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a sales enquiry.");
                    return View(quotationReport);
                }
                quotationReport.IsPositive = quotationReport.IsPositiveBool ? (byte)1 : (byte)0;

                // Set the CreatedUserId and CreatedDateTime
                quotationReport.CreatedUserId = userId;
                quotationReport.CreatedDateTime = DateTime.Now;
                quotationReport.FyYear = GetFyId(compId);

                // Insert into QuotationReport
                _context.Add(quotationReport);
                await _context.SaveChangesAsync();

                // If IsPositive == 1, also insert into PositiveEnquiry
                if (quotationReport.IsPositive == 1)
                {
                    var positiveEnquiry = new PositiveEnquiry
                    {
                        QuotationDate = quotationReport.QuotationDate,
                        CompanyNameId = compId,
                        ProductDetails = quotationReport.ProductDetails,
                        CustomerDetails = quotationReport.CustomerDetails,
                        QuotationValue = quotationReport.QuotationValue,
                        CreatedDateTime = DateTime.Now,
                        CreatedUserId = userId,
                        FyYear = quotationReport.FyYear
                    };

                    _context.Add(positiveEnquiry);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Quotation Report created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            ViewBag.ActiveFinancialYear = GetActiveFinancialYear(compId);
            return View(quotationReport);
        }



        // GET: QuotationReports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var quotationReport = await _context.QuotationReport.FirstOrDefaultAsync(qr => qr.Id == id && qr.CompanyNameId == compId);
            if (quotationReport == null) return NotFound();

            ViewBag.Companies = _context.CompanyName
                .Where(c => c.Id == compId || c.IsActive == 1)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
                .ToList();

            return View(quotationReport);
        }

        // POST: QuotationReports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationDate,CompanyNameId,ProductDetails,CustomerDetails,QuotationValue,Remarks,IsPositive,CreatedDateTime,CreatedUserId")] QuotationReport quotationReport)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            quotationReport.CompanyNameId = compId;

            if (id != quotationReport.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    quotationReport.IsPositive = quotationReport.IsPositiveBool ? (byte)1 : (byte)0;
                    var existingQuotation = await _context.QuotationReport.FirstOrDefaultAsync(qr => qr.Id == id && qr.CompanyNameId == compId);
                    if (existingQuotation == null) return NotFound();

                    quotationReport.CreatedUserId = existingQuotation.CreatedUserId;
                    quotationReport.CreatedDateTime = existingQuotation.CreatedDateTime;
                    quotationReport.FyYear = existingQuotation.FyYear != 0 ? existingQuotation.FyYear : GetFyId(compId);

                    // Update QuotationReport
                    _context.Entry(existingQuotation).CurrentValues.SetValues(quotationReport);
                    await _context.SaveChangesAsync();

                    // Check if IsPositive == 1 (Update or Insert into PositiveEnquiry)
                    var existingPositiveEnquiry = await _context.PositiveEnquiry.FirstOrDefaultAsync(pe => pe.Id == quotationReport.Id && pe.CompanyNameId == compId);
                    if (quotationReport.IsPositive == 1)
                    {
                        if (existingPositiveEnquiry == null)
                        {
                            var positiveEnquiry = new PositiveEnquiry
                            {
                                QuotationDate = quotationReport.QuotationDate,
                                CompanyNameId = compId,
                                ProductDetails = quotationReport.ProductDetails,
                                CustomerDetails = quotationReport.CustomerDetails,
                                QuotationValue = quotationReport.QuotationValue,
                                CreatedDateTime = DateTime.Now,
                                CreatedUserId = quotationReport.CreatedUserId,
                                FyYear = quotationReport.FyYear
                            };
                            _context.Add(positiveEnquiry);
                        }
                        else
                        {
                            existingPositiveEnquiry.QuotationDate = quotationReport.QuotationDate;
                            existingPositiveEnquiry.CompanyNameId = compId;
                            existingPositiveEnquiry.ProductDetails = quotationReport.ProductDetails;
                            existingPositiveEnquiry.CustomerDetails = quotationReport.CustomerDetails;
                            existingPositiveEnquiry.QuotationValue = quotationReport.QuotationValue;
                            existingPositiveEnquiry.FyYear = quotationReport.FyYear;
                            _context.Update(existingPositiveEnquiry);
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        if (existingPositiveEnquiry != null)
                        {
                            _context.PositiveEnquiry.Remove(existingPositiveEnquiry);
                            await _context.SaveChangesAsync();
                        }
                    }
                    TempData["SuccessMessage"] = "Quotation Report updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuotationReportExists(quotationReport.Id, compId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Companies = _context.CompanyName
              .Where(c => c.Id == compId || c.IsActive == 1)
              .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
              .ToList();

            return View(quotationReport);
        }

        // GET: QuotationReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var quotationReport = await _context.QuotationReport
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);
            if (quotationReport == null) return NotFound();

            return View(quotationReport);
        }

        // POST: QuotationReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var quotationReport = await _context.QuotationReport.FirstOrDefaultAsync(qr => qr.Id == id && qr.CompanyNameId == compId);
            if (quotationReport != null)
            {
                _context.QuotationReport.Remove(quotationReport);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Quotation Report deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool QuotationReportExists(int id, int companyId)
        {
            return _context.QuotationReport.Any(e => e.Id == id && e.CompanyNameId == companyId);
        }
    }
}
