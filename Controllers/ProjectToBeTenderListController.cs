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
    public class ProjectToBeTenderListController : Controller
    {
        private readonly KLENZDbContext _context;

        public ProjectToBeTenderListController(KLENZDbContext context)
        {
            _context = context;
        }

        // GET: ProjectToBeTenderList
        public async Task<IActionResult> Index()
        {
            var kLENZDbContext = _context.Project_ToBeTenderList.Include(p => p.Company).Include(p => p.FinancialYear)
                .Include(p => p.GSTType).OrderByDescending(p=> p.CreatedDateTime);
            return View(await kLENZDbContext.ToListAsync());
        }

        // GET: ProjectToBeTenderList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project_ToBeTenderList = await _context.Project_ToBeTenderList
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project_ToBeTenderList == null)
            {
                return NotFound();
            }

            return View(project_ToBeTenderList);
        }

        // GET: ProjectToBeTenderList/Create
        public IActionResult Create()
        {
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "FullName");
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear");
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "Id");
            return View();
        }

        // POST: ProjectToBeTenderList/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost,GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_ToBeTenderList project_ToBeTenderList)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project_ToBeTenderList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "FullName", project_ToBeTenderList.CompanyNameId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", project_ToBeTenderList.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "Id", project_ToBeTenderList.GSTTypeId);
            return View(project_ToBeTenderList);
        }

        // GET: ProjectToBeTenderList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project_ToBeTenderList = await _context.Project_ToBeTenderList.FindAsync(id);
            if (project_ToBeTenderList == null)
            {
                return NotFound();
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "FullName", project_ToBeTenderList.CompanyNameId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", project_ToBeTenderList.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "Id", project_ToBeTenderList.GSTTypeId);
            return View(project_ToBeTenderList);
        }

        // POST: ProjectToBeTenderList/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost,GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_ToBeTenderList project_ToBeTenderList)
        {
            if (id != project_ToBeTenderList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project_ToBeTenderList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Project_ToBeTenderListExists(project_ToBeTenderList.Id))
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
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName, "Id", "FullName", project_ToBeTenderList.CompanyNameId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", project_ToBeTenderList.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "Id", project_ToBeTenderList.GSTTypeId);
            return View(project_ToBeTenderList);
        }

        // GET: ProjectToBeTenderList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project_ToBeTenderList = await _context.Project_ToBeTenderList
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project_ToBeTenderList == null)
            {
                return NotFound();
            }

            return View(project_ToBeTenderList);
        }

        // POST: ProjectToBeTenderList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project_ToBeTenderList = await _context.Project_ToBeTenderList.FindAsync(id);
            if (project_ToBeTenderList != null)
            {
                _context.Project_ToBeTenderList.Remove(project_ToBeTenderList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Project_ToBeTenderListExists(int id)
        {
            return _context.Project_ToBeTenderList.Any(e => e.Id == id);
        }
    }
}
