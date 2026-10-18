using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KLENZ.Data;
using KLENZ.Models;
using Microsoft.AspNetCore.Identity;

namespace KLENZ.Controllers
{
    public class FinancialYearsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinancialYearsController(KLENZDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: FinancialYears
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var defaultUserName = currentUser?.UserName ?? currentUser?.Email ?? "System";

            var financialYears = await _context.FinancialYear
                .Select(fy => new
                {
                    fy.Id,
                    fy.StartDate,
                    fy.EndDate,
                    fy.FyYear,
                    fy.IsActive,
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

            var model = financialYears.Select(fy => new FinancialYear
            {
                Id = fy.Id,
                StartDate = fy.StartDate,
                EndDate = fy.EndDate,
                FyYear = string.IsNullOrWhiteSpace(fy.FyYear) ? $"{fy.StartDate.Year} - {fy.EndDate.Year}" : fy.FyYear,
                IsActive = fy.IsActive,
                CreatedDateTime = fy.CreatedDateTime,
                CreatedUserId = fy.CreatedUserId,
                CreatedUserName = string.IsNullOrWhiteSpace(fy.CreatedUserName) ? defaultUserName : fy.CreatedUserName
            });

            return View(model);
        }

        // GET: FinancialYears/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var financialYear = await _context.FinancialYear
                .FirstOrDefaultAsync(m => m.Id == id);
            if (financialYear == null)
            {
                return NotFound();
            }

            return View(financialYear);
        }

        // GET: FinancialYears/Create
        public IActionResult Create()
        {
            var model = new FinancialYear
            {
                StartDate = new DateTime(DateTime.Now.Year, 4, 1),
                EndDate = new DateTime(DateTime.Now.Year + 1, 3, 31),
                IsActive = 0
            };
            return View(model);
        }

        // POST: FinancialYears/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,IsActiveBool")] FinancialYear financialYear)
        {
            if (ModelState.IsValid)
            {
                var fyYearString = $"{financialYear.StartDate.Year} - {financialYear.EndDate.Year}";

                // Duplicate check
                bool isDuplicate = await _context.FinancialYear.AnyAsync(fy =>
                    fy.FyYear == fyYearString ||
                    (fy.StartDate.Date == financialYear.StartDate.Date && fy.EndDate.Date == financialYear.EndDate.Date));

                if (isDuplicate)
                {
                    ModelState.AddModelError("StartDate", $"Financial Year '{fyYearString}' already exists.");
                    TempData["ErrorMessage"] = $"Financial Year '{fyYearString}' already exists!";
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

                // Single active financial year logic: deactivate all others if this one is active
                if (financialYear.IsActive == 1)
                {
                    var existingActive = await _context.FinancialYear.Where(fy => fy.IsActive == 1).ToListAsync();
                    foreach (var fy in existingActive)
                    {
                        fy.IsActive = 0;
                    }
                }

                _context.Add(financialYear);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Financial Year '{financialYear.FyYear}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(financialYear);
        }

        // GET: FinancialYears/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var financialYear = await _context.FinancialYear.FindAsync(id);
            if (financialYear == null)
            {
                return NotFound();
            }
            return View(financialYear);
        }

        // POST: FinancialYears/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,IsActiveBool")] FinancialYear financialYear)
        {
            if (id != financialYear.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingFyYear = await _context.FinancialYear.FindAsync(id);
                if (existingFyYear == null)
                {
                    return NotFound();
                }

                var fyYearString = $"{financialYear.StartDate.Year} - {financialYear.EndDate.Year}";

                // Duplicate check
                bool isDuplicate = await _context.FinancialYear.AnyAsync(fy =>
                    fy.Id != id && (fy.FyYear == fyYearString ||
                    (fy.StartDate.Date == financialYear.StartDate.Date && fy.EndDate.Date == financialYear.EndDate.Date)));

                if (isDuplicate)
                {
                    ModelState.AddModelError("StartDate", $"Financial Year '{fyYearString}' already exists.");
                    TempData["ErrorMessage"] = $"Financial Year '{fyYearString}' already exists!";
                    return View(financialYear);
                }

                try
                {
                    existingFyYear.StartDate = financialYear.StartDate;
                    existingFyYear.EndDate = financialYear.EndDate;
                    existingFyYear.FyYear = fyYearString;
                    existingFyYear.IsActive = financialYear.IsActiveBool ? (byte)1 : (byte)0;

                    // Single active financial year logic: deactivate all others if this one is active
                    if (existingFyYear.IsActive == 1)
                    {
                        var otherActive = await _context.FinancialYear.Where(fy => fy.Id != id && fy.IsActive == 1).ToListAsync();
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
                    if (!FinancialYearExists(financialYear.Id))
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
            var allYears = await _context.FinancialYear.ToListAsync();
            var targetYear = allYears.FirstOrDefault(fy => fy.Id == id);
            if (targetYear == null)
            {
                return NotFound();
            }

            foreach (var fy in allYears)
            {
                fy.IsActive = (fy.Id == id) ? (byte)1 : (byte)0;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Financial Year '{targetYear.FyYear}' is now Active!";
            return RedirectToAction(nameof(Index));
        }

        // GET: FinancialYears/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var financialYear = await _context.FinancialYear
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var financialYear = await _context.FinancialYear.FindAsync(id);
            if (financialYear != null)
            {
                _context.FinancialYear.Remove(financialYear);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FinancialYearExists(int id)
        {
            return _context.FinancialYear.Any(e => e.Id == id);
        }
    }
}
