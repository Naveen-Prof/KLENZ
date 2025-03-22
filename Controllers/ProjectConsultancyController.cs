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
    public class ProjectConsultancyController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectConsultancyController(KLENZDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ProjectConsultancy
        public async Task<IActionResult> Index()
        {
            var kLENZDbContext = _context.Project_Consultancy.Include(p => p.Company).Include(p => p.FinancialYear).Include(p => p.GSTType);
            return View(await kLENZDbContext.ToListAsync());
        }

        // GET: ProjectConsultancy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project_Consultancy = await _context.Project_Consultancy
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project_Consultancy == null)
            {
                return NotFound();
            }

            return View(project_Consultancy);
        }

        // GET: ProjectConsultancy/Create
        public IActionResult Create()
        {
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "ShortName");
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear");
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType");
            return View();
        }

        // POST: ProjectConsultancy/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost" +
            ",GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_Consultancy project_Consultancy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project_Consultancy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "ShortName", project_Consultancy.CompanyNameId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", project_Consultancy.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", project_Consultancy.GSTTypeId);
            return View(project_Consultancy);
        }

        // GET: ProjectConsultancy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project_Consultancy = await _context.Project_Consultancy.FindAsync(id);
            if (project_Consultancy == null)
            {
                return NotFound();
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "ShortName", project_Consultancy.CompanyNameId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", project_Consultancy.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", project_Consultancy.GSTTypeId);
            return View(project_Consultancy);
        }

        // POST: ProjectConsultancy/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost" +
            ",GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_Consultancy project_Consultancy)
        {
            if (id != project_Consultancy.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project_Consultancy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Project_ConsultancyExists(project_Consultancy.Id))
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
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "ShortName", project_Consultancy.CompanyNameId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", project_Consultancy.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", project_Consultancy.GSTTypeId);
            return View(project_Consultancy);
        }

        // GET: ProjectConsultancy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project_Consultancy = await _context.Project_Consultancy
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project_Consultancy == null)
            {
                return NotFound();
            }

            return View(project_Consultancy);
        }

        // POST: ProjectConsultancy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project_Consultancy = await _context.Project_Consultancy.FindAsync(id);
            if (project_Consultancy != null)
            {
                _context.Project_Consultancy.Remove(project_Consultancy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Project_ConsultancyExists(int id)
        {
            return _context.Project_Consultancy.Any(e => e.Id == id);
        }
    }
}
