using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KLENZ.Data;
using KLENZ.Models;

namespace KLENZ.Controllers
{
    public class CompanyNamesController : Controller
    {
        private readonly KLENZDbContext _context;

        public CompanyNamesController(KLENZDbContext context)
        {
            _context = context;
        }

        // GET: CompanyNames
        public async Task<IActionResult> Index()
        {
            return View(await _context.CompanyName.OrderByDescending(c => c.CreatedDateTime).ToListAsync());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,ShortName,IsActive,CreatedDateTime,CreatedUserId")] CompanyName companyName)
        {
            if (ModelState.IsValid)
            {
                _context.Add(companyName);
                await _context.SaveChangesAsync();
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
