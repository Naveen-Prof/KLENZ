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
                .OrderByDescending(c => c.CreatedDateTime)
                .Select(c => new CompanyName
                {
                    Id = c.Id,
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

            return View(companyName);
        }

        // GET: CompanyNames/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CompanyNames/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,ShortName,IsActive,CreatedDateTime,CreatedUserId")] CompanyName companyName)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    companyName.CreatedUserId = userId;
                }
                else
                {
                    companyName.CreatedUserId = null;
                }
                companyName.CreatedDateTime = DateTime.Now;

                _context.Add(companyName);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Company Name created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(companyName);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,ShortName,IsActive,CreatedDateTime,CreatedUserId")] CompanyName companyName)
        {
            if (id != companyName.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyName);
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
