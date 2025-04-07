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
using Microsoft.Build.ObjectModelRemoting;

namespace KLENZ.Controllers
{
    public class FinancialYearsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FinancialYearsController(KLENZDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: FinancialYears
        public async Task<IActionResult> Index()
        {
            var financialYears = await _context.FinancialYear
                .Select(fy => new
                {
                    fy.Id,
                    fy.FyYear,
                    fy.IsActive,
                    fy.CreatedDateTime,
                    fy.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == fy.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .OrderByDescending(fy => fy.CreatedDateTime)
                .ToListAsync();

            var model = financialYears.Select(fy => new FinancialYear
            {
                Id = fy.Id,
                FyYear = fy.FyYear,
                IsActive = fy.IsActive,
                CreatedDateTime = fy.CreatedDateTime,
                CreatedUserId = fy.CreatedUserId,
                CreatedUserName = fy.CreatedUserName // Add this property to your model
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
            return View();
        }

        // POST: FinancialYears/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,IsActive,CreatedDateTime,CreatedUserId")] FinancialYear financialYear)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a sales enquiry.");
                    return View(financialYear);
                }

                // Set the CreatedUserId and CreatedDateTime
                financialYear.CreatedUserId = userId;
                financialYear.CreatedDateTime = DateTime.Now;
                _context.Add(financialYear);
                await _context.SaveChangesAsync();
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FyYear,IsActiveBool")] FinancialYear financialYear)
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

                try
                {
                    // Preserve CreatedUserId and CreatedDateTime
                    existingFyYear.FyYear = financialYear.FyYear;
                    existingFyYear.IsActive = financialYear.IsActiveBool ? (byte)1 : (byte)0;

                    _context.Entry(existingFyYear).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

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
