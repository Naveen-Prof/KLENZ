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
    public class GSTTypesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GSTTypesController(KLENZDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GSTTypes
        public async Task<IActionResult> Index()
        {
            var gstList = await _context.GSTTypes
                .OrderByDescending(g => g.CreatedDateTime)
                .Select(g => new GSTTypes
                {
                    Id = g.Id,
                    GSTType = g.GSTType,
                    CreatedDateTime = g.CreatedDateTime,
                    CreatedUserId = g.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == g.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System"
                })
                .ToListAsync();

            return View(gstList);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GSTType,CreatedDateTime,CreatedUserId")] GSTTypes gSTTypes)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    gSTTypes.CreatedUserId = userId;
                }
                else
                {
                    gSTTypes.CreatedUserId = null;
                }
                gSTTypes.CreatedDateTime = DateTime.Now;

                _context.Add(gSTTypes);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "GST Type created successfully!";
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
