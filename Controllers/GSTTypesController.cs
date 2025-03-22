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
    public class GSTTypesController : Controller
    {
        private readonly KLENZDbContext _context;

        public GSTTypesController(KLENZDbContext context)
        {
            _context = context;
        }

        // GET: GSTTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.GSTTypes.ToListAsync());
        }

        // GET: GSTTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gSTTypes = await _context.GSTTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gSTTypes == null)
            {
                return NotFound();
            }

            return View(gSTTypes);
        }

        // GET: GSTTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GSTTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GSTType,CreatedDateTime,CreatedUserId")] GSTTypes gSTTypes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gSTTypes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gSTTypes);
        }

        // GET: GSTTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gSTTypes = await _context.GSTTypes.FindAsync(id);
            if (gSTTypes == null)
            {
                return NotFound();
            }
            return View(gSTTypes);
        }

        // POST: GSTTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GSTType,CreatedDateTime,CreatedUserId")] GSTTypes gSTTypes)
        {
            if (id != gSTTypes.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gSTTypes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GSTTypesExists(gSTTypes.Id))
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
            return View(gSTTypes);
        }

        // GET: GSTTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gSTTypes = await _context.GSTTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gSTTypes == null)
            {
                return NotFound();
            }

            return View(gSTTypes);
        }

        // POST: GSTTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gSTTypes = await _context.GSTTypes.FindAsync(id);
            if (gSTTypes != null)
            {
                _context.GSTTypes.Remove(gSTTypes);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GSTTypesExists(int id)
        {
            return _context.GSTTypes.Any(e => e.Id == id);
        }
    }
}
