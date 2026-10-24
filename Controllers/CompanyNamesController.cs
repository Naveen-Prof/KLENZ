using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using KLENZ.Data;
using KLENZ.Models;

namespace KLENZ.Controllers
{
    public class CompanyNamesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyNamesController(KLENZDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CompanyNames
        public async Task<IActionResult> Index()
        {
            var companies = await _context.CompanyName
                .OrderBy(c => c.SequenceNo)
                .ThenBy(c => c.ShortName)
                .Select(c => new CompanyName
                {
                    Id = c.Id,
                    SequenceNo = c.SequenceNo,
                    FullName = c.FullName,
                    ShortName = c.ShortName,
                    IsActive = c.IsActive,
                    CreatedDateTime = c.CreatedDateTime,
                    CreatedUserId = c.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == c.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System"
                })
                .ToListAsync();

            return View(companies);
        }

        // GET: CompanyNames/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyName = await _context.CompanyName
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyName == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(companyName.CreatedUserId))
            {
                companyName.CreatedUserName = (await _context.Users
                    .Where(u => u.Id == companyName.CreatedUserId)
                    .Select(u => u.UserName ?? u.Email)
                    .FirstOrDefaultAsync()) ?? "System";
            }
            else
            {
                companyName.CreatedUserName = "System";
            }

            return View(companyName);
        }

        // GET: CompanyNames/Create (Provision removed - companies created via query)
        public IActionResult Create()
        {
            TempData["InfoMessage"] = "Company creation via UI is disabled. Companies are created via database query.";
            return RedirectToAction(nameof(Index));
        }

        // POST: CompanyNames/Create (Provision removed - companies created via query)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,SequenceNo,FullName,ShortName,IsActive")] CompanyName companyName)
        {
            TempData["InfoMessage"] = "Company creation via UI is disabled. Companies are created via database query.";
            return RedirectToAction(nameof(Index));
        }

        // GET: CompanyNames/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyName = await _context.CompanyName.FindAsync(id);
            if (companyName == null)
            {
                return NotFound();
            }
            return View(companyName);
        }

        // POST: CompanyNames/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SequenceNo,FullName,ShortName,IsActive,CreatedDateTime,CreatedUserId")] CompanyName companyName)
        {
            if (id != companyName.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.CompanyName.FindAsync(id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    int oldSequenceNo = existing.SequenceNo;
                    int newSequenceNo = companyName.SequenceNo;

                    // If sequence number changed, swap with any conflicting company
                    if (oldSequenceNo != newSequenceNo)
                    {
                        var conflictingCompanies = await _context.CompanyName
                            .Where(c => c.Id != id && c.SequenceNo == newSequenceNo)
                            .ToListAsync();

                        foreach (var conflict in conflictingCompanies)
                        {
                            conflict.SequenceNo = oldSequenceNo;
                        }
                    }

                    existing.SequenceNo = newSequenceNo;
                    existing.FullName = companyName.FullName;
                    existing.ShortName = companyName.ShortName;
                    existing.IsActive = companyName.IsActive;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Company Name updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyNameExists(companyName.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(companyName);
        }

        // GET: CompanyNames/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyName = await _context.CompanyName
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyName == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(companyName.CreatedUserId))
            {
                companyName.CreatedUserName = (await _context.Users
                    .Where(u => u.Id == companyName.CreatedUserId)
                    .Select(u => u.UserName ?? u.Email)
                    .FirstOrDefaultAsync()) ?? "System";
            }
            else
            {
                companyName.CreatedUserName = "System";
            }

            return View(companyName);
        }

        // POST: CompanyNames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyName = await _context.CompanyName.FindAsync(id);
            if (companyName != null)
            {
                _context.CompanyName.Remove(companyName);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyNameExists(int id)
        {
            return _context.CompanyName.Any(e => e.Id == id);
        }
    }
}
