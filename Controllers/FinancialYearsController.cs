using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KLENZ.Data;
using KLENZ.Models;
using KLENZ.Services;
using Microsoft.AspNetCore.Identity;

namespace KLENZ.Controllers
{
    public class FinancialYearsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyContext _companyContext;

        public FinancialYearsController(
            KLENZDbContext context, 
            UserManager<ApplicationUser> userManager,
            ICompanyContext companyContext)
        {
            _context = context;
            _userManager = userManager;
            _companyContext = companyContext;
        }

        // GET: FinancialYears
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var currentUser = await _userManager.GetUserAsync(User);
            var defaultUserName = currentUser?.UserName ?? currentUser?.Email ?? "System";

            var financialYears = await _context.FinancialYear
                .Where(fy => fy.CompanyNameId == currentCompId)
                .Select(fy => new
                {
                    fy.Id,
                    fy.StartDate,
                    fy.EndDate,
                    fy.FyYear,
                    fy.IsActive,
                    fy.CompanyNameId,
                    fy.CreatedDateTime,
                    fy.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == fy.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault()
                })
                .OrderByDescending(fy => fy.IsActive)
                .ThenByDescending(fy => fy.CreatedDateTime)
                .ToListAsync();

            var currentCompanyName = _companyContext.CurrentCompanyShortName ?? _companyContext.CurrentCompanyName ?? "Selected Company";
            ViewBag.CurrentCompanyName = currentCompanyName;

            var model = financialYears.Select(fy => new FinancialYear
            {
                Id = fy.Id,
                StartDate = fy.StartDate,
                EndDate = fy.EndDate,
                FyYear = string.IsNullOrWhiteSpace(fy.FyYear) ? $"{fy.StartDate.Year} - {fy.EndDate.Year}" : fy.FyYear,
                IsActive = fy.IsActive,
                CompanyNameId = fy.CompanyNameId,
                CompanyNameStr = currentCompanyName,
                CreatedDateTime = fy.CreatedDateTime,
                CreatedUserId = fy.CreatedUserId,
                CreatedUserName = string.IsNullOrWhiteSpace(fy.CreatedUserName) ? defaultUserName : fy.CreatedUserName
            });

            return View(model);
        }

        // GET: FinancialYears/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null)
            {
                return NotFound();
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var financialYear = await _context.FinancialYear
                .Include(fy => fy.Company)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == currentCompId);

            if (financialYear == null)
            {
                return NotFound();
            }

            financialYear.CompanyNameStr = financialYear.Company?.ShortName ?? _companyContext.CurrentCompanyShortName;
            return View(financialYear);
        }

        // GET: FinancialYears/Create
        public IActionResult Create()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var model = new FinancialYear
            {
                StartDate = new DateTime(DateTime.Now.Year, 4, 1),
                EndDate = new DateTime(DateTime.Now.Year + 1, 3, 31),
                IsActive = 0,
                CompanyNameId = currentCompId,
                CompanyNameStr = _companyContext.CurrentCompanyShortName ?? _companyContext.CurrentCompanyName
            };

            ViewBag.CurrentCompanyName = model.CompanyNameStr;
            return View(model);
        }

        // POST: FinancialYears/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,IsActiveBool")] FinancialYear financialYear)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            financialYear.CompanyNameId = currentCompId;
            ViewBag.CurrentCompanyName = _companyContext.CurrentCompanyShortName ?? _companyContext.CurrentCompanyName;

            if (ModelState.IsValid)
            {
                var fyYearString = $"{financialYear.StartDate.Year} - {financialYear.EndDate.Year}";

                // Duplicate check scoped to current company
                bool isDuplicate = await _context.FinancialYear.AnyAsync(fy =>
                    fy.CompanyNameId == currentCompId &&
                    (fy.FyYear == fyYearString ||
                    (fy.StartDate.Date == financialYear.StartDate.Date && fy.EndDate.Date == financialYear.EndDate.Date)));

                if (isDuplicate)
                {
                    ModelState.AddModelError("StartDate", $"Financial Year '{fyYearString}' already exists for {_companyContext.CurrentCompanyShortName}.");
                    TempData["ErrorMessage"] = $"Financial Year '{fyYearString}' already exists for {_companyContext.CurrentCompanyShortName}!";
                    return View(financialYear);
                }

                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    financialYear.CreatedUserId = userId;
                }
                else
                {
                    financialYear.CreatedUserId = null;
                }
                financialYear.CreatedDateTime = DateTime.Now;
                financialYear.IsActive = financialYear.IsActiveBool ? (byte)1 : (byte)0;
                financialYear.FyYear = fyYearString;

                // Scoped single active financial year logic: deactivate other financial years for THIS company only
                if (financialYear.IsActive == 1)
                {
                    var existingActive = await _context.FinancialYear
                        .Where(fy => fy.CompanyNameId == currentCompId && fy.IsActive == 1)
                        .ToListAsync();

                    foreach (var fy in existingActive)
                    {
                        fy.IsActive = 0;
                    }
                }

                _context.Add(financialYear);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Financial Year '{financialYear.FyYear}' created for {_companyContext.CurrentCompanyShortName}!";
                return RedirectToAction(nameof(Index));
            }
            return View(financialYear);
        }

        // GET: FinancialYears/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null)
            {
                return NotFound();
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var financialYear = await _context.FinancialYear
                .FirstOrDefaultAsync(fy => fy.Id == id && fy.CompanyNameId == currentCompId);

            if (financialYear == null)
            {
                return NotFound();
            }

            ViewBag.CurrentCompanyName = _companyContext.CurrentCompanyShortName ?? _companyContext.CurrentCompanyName;
            return View(financialYear);
        }

        // POST: FinancialYears/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,IsActiveBool")] FinancialYear financialYear)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            if (id != financialYear.Id)
            {
                return NotFound();
            }

            ViewBag.CurrentCompanyName = _companyContext.CurrentCompanyShortName ?? _companyContext.CurrentCompanyName;

            if (ModelState.IsValid)
            {
                var existingFyYear = await _context.FinancialYear
                    .FirstOrDefaultAsync(fy => fy.Id == id && fy.CompanyNameId == currentCompId);

                if (existingFyYear == null)
                {
                    return NotFound();
                }

                var fyYearString = $"{financialYear.StartDate.Year} - {financialYear.EndDate.Year}";

                // Duplicate check scoped to current company
                bool isDuplicate = await _context.FinancialYear.AnyAsync(fy =>
                    fy.CompanyNameId == currentCompId &&
                    fy.Id != id && 
                    (fy.FyYear == fyYearString ||
                    (fy.StartDate.Date == financialYear.StartDate.Date && fy.EndDate.Date == financialYear.EndDate.Date)));

                if (isDuplicate)
                {
                    ModelState.AddModelError("StartDate", $"Financial Year '{fyYearString}' already exists for {_companyContext.CurrentCompanyShortName}.");
                    TempData["ErrorMessage"] = $"Financial Year '{fyYearString}' already exists for {_companyContext.CurrentCompanyShortName}!";
                    return View(financialYear);
                }

                try
                {
                    existingFyYear.StartDate = financialYear.StartDate;
                    existingFyYear.EndDate = financialYear.EndDate;
                    existingFyYear.FyYear = fyYearString;
                    existingFyYear.IsActive = financialYear.IsActiveBool ? (byte)1 : (byte)0;

                    // Scoped single active financial year logic: deactivate other financial years for THIS company only
                    if (existingFyYear.IsActive == 1)
                    {
                        var otherActive = await _context.FinancialYear
                            .Where(fy => fy.CompanyNameId == currentCompId && fy.Id != id && fy.IsActive == 1)
                            .ToListAsync();

                        foreach (var fy in otherActive)
                        {
                            fy.IsActive = 0;
                        }
                    }

                    _context.Entry(existingFyYear).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Financial Year '{existingFyYear.FyYear}' updated successfully!";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FinancialYearExists(financialYear.Id, currentCompId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(financialYear);
        }

        // POST: FinancialYears/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var companyYears = await _context.FinancialYear
                .Where(fy => fy.CompanyNameId == currentCompId)
                .ToListAsync();

            var targetYear = companyYears.FirstOrDefault(fy => fy.Id == id);
            if (targetYear == null)
            {
                return NotFound();
            }

            // Deactivate all for this company, activate target
            foreach (var fy in companyYears)
            {
                fy.IsActive = (fy.Id == id) ? (byte)1 : (byte)0;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Financial Year '{targetYear.FyYear}' is now Active for {_companyContext.CurrentCompanyShortName}!";
            return RedirectToAction(nameof(Index));
        }

        // GET: FinancialYears/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null)
            {
                return NotFound();
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var financialYear = await _context.FinancialYear
                .Include(fy => fy.Company)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == currentCompId);

            if (financialYear == null)
            {
                return NotFound();
            }

            return View(financialYear);
        }

        // POST: FinancialYears/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int currentCompId = _companyContext.CurrentCompanyId!.Value;
            var financialYear = await _context.FinancialYear
                .FirstOrDefaultAsync(fy => fy.Id == id && fy.CompanyNameId == currentCompId);

            if (financialYear != null)
            {
                _context.FinancialYear.Remove(financialYear);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Financial Year deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FinancialYearExists(int id, int companyId)
        {
            return _context.FinancialYear.Any(e => e.Id == id && e.CompanyNameId == companyId);
        }
    }
}
