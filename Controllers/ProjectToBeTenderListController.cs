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
    public class ProjectToBeTenderListController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly KLENZ.Services.ICompanyContext _companyContext;

        public ProjectToBeTenderListController(KLENZDbContext context, UserManager<ApplicationUser> userManager, KLENZ.Services.ICompanyContext companyContext)
        {
            _context = context;
            _userManager = userManager;
            _companyContext = companyContext;
        }

        // GET: ProjectToBeTenderList
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var list = await _context.Project_ToBeTenderList
                .Where(p => p.CompanyNameId == compId)
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .OrderByDescending(p => p.CreatedDateTime)
                .Select(p => new Project_ToBeTenderList
                {
                    Id = p.Id,
                    FyYear = p.FyYear,
                    WorkOrderDate = p.WorkOrderDate,
                    CompanyNameId = p.CompanyNameId,
                    CustomerDetails = p.CustomerDetails,
                    WorkDetails = p.WorkDetails,
                    ProjectCost = p.ProjectCost,
                    GSTTypeId = p.GSTTypeId,
                    Total = p.Total,
                    WorkDuration = p.WorkDuration,
                    Remarks = p.Remarks,
                    CreatedDateTime = p.CreatedDateTime,
                    CreatedUserId = p.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == p.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System",
                    FinancialYear = p.FinancialYear,
                    Company = p.Company,
                    GSTType = p.GSTType
                })
                .ToListAsync();

            return View(list);
        }

        // GET: ProjectToBeTenderList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var project_ToBeTenderList = await _context.Project_ToBeTenderList
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);
            if (project_ToBeTenderList == null)
            {
                return NotFound();
            }

            return View(project_ToBeTenderList);
        }

        // GET: ProjectToBeTenderList/Create
        public IActionResult Create()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName", compId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear");
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType");
            return View();
        }

        // POST: ProjectToBeTenderList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost,GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_ToBeTenderList project_ToBeTenderList)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            project_ToBeTenderList.CompanyNameId = compId;

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                project_ToBeTenderList.CreatedUserId = userId;
                project_ToBeTenderList.CreatedDateTime = DateTime.Now;

                _context.Add(project_ToBeTenderList);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "To Be Tender Project created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName", compId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", project_ToBeTenderList.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", project_ToBeTenderList.GSTTypeId);
            return View(project_ToBeTenderList);
        }

        // GET: ProjectToBeTenderList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var project_ToBeTenderList = await _context.Project_ToBeTenderList.FirstOrDefaultAsync(p => p.Id == id && p.CompanyNameId == compId);
            if (project_ToBeTenderList == null)
            {
                return NotFound();
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName", compId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", project_ToBeTenderList.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", project_ToBeTenderList.GSTTypeId);
            return View(project_ToBeTenderList);
        }

        // POST: ProjectToBeTenderList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost,GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_ToBeTenderList project_ToBeTenderList)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            project_ToBeTenderList.CompanyNameId = compId;

            if (id != project_ToBeTenderList.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Project_ToBeTenderList.FirstOrDefaultAsync(p => p.Id == id && p.CompanyNameId == compId);
                    if (existing == null) return NotFound();

                    project_ToBeTenderList.CreatedUserId = existing.CreatedUserId;
                    project_ToBeTenderList.CreatedDateTime = existing.CreatedDateTime;

                    _context.Entry(existing).CurrentValues.SetValues(project_ToBeTenderList);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "To Be Tender Project updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Project_ToBeTenderListExists(project_ToBeTenderList.Id, compId))
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
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName", compId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", project_ToBeTenderList.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", project_ToBeTenderList.GSTTypeId);
            return View(project_ToBeTenderList);
        }

        // GET: ProjectToBeTenderList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var project_ToBeTenderList = await _context.Project_ToBeTenderList
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);
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
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var project_ToBeTenderList = await _context.Project_ToBeTenderList.FirstOrDefaultAsync(p => p.Id == id && p.CompanyNameId == compId);
            if (project_ToBeTenderList != null)
            {
                _context.Project_ToBeTenderList.Remove(project_ToBeTenderList);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "To Be Tender Project deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool Project_ToBeTenderListExists(int id, int companyId)
        {
            return _context.Project_ToBeTenderList.Any(e => e.Id == id && e.CompanyNameId == companyId);
        }
    }
}
