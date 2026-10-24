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
    public class ProjectListsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly KLENZ.Services.ICompanyContext _companyContext;

        public ProjectListsController(KLENZDbContext context, UserManager<ApplicationUser> userManager, KLENZ.Services.ICompanyContext companyContext)
        {
            _context = context;
            _userManager = userManager;
            _companyContext = companyContext;
        }

        // GET: ProjectLists
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var projectLists = await _context.ProjectList
                .Where(p => p.CompanyNameId == compId)
                .Include(p => p.FinancialYear)
                .Include(c => c.Company)
                .OrderByDescending(p => p.CreatedDateTime)
                .Select(p => new ProjectList
                {
                    Id = p.Id,
                    FyYear = p.FyYear,
                    FinancialYear = p.FinancialYear,
                    WorkOrderDate = p.WorkOrderDate,
                    CompanyNameId = p.CompanyNameId,
                    Company = p.Company,
                    CustomerDetails = p.CustomerDetails,
                    WorkDetails = p.WorkDetails,
                    WorkOrderValue = p.WorkOrderValue,
                    Remarks = p.Remarks,
                    CreatedDateTime = p.CreatedDateTime,
                    CreatedUserId = p.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == p.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System"
                })
                .ToListAsync();

            return View(projectLists ?? new List<ProjectList>());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var projectList = await _context.ProjectList
                .Include(p => p.FinancialYear)
                .Include(p => p.Company)
                .Where(p => p.Id == id && p.CompanyNameId == compId)
                .Select(p => new
                {
                    p.Id,
                    p.WorkOrderDate,
                    p.CustomerDetails,
                    p.WorkDetails,
                    p.WorkOrderValue,
                    p.Remarks,
                    p.CreatedDateTime,
                    p.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == p.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    CompanyShortName = p.Company != null ? p.Company.ShortName : "N/A",
                    FinancialYear = _context.FinancialYear
                        .Where(f => f.Id == p.FyYear)
                        .Select(f => f.FyYear)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (projectList == null) return NotFound();

            var viewModel = new ProjectList
            {
                Id = projectList.Id,
                FinancialYearStr = projectList.FinancialYear,
                WorkOrderDate = projectList.WorkOrderDate,
                CustomerDetails = projectList.CustomerDetails,
                WorkDetails = projectList.WorkDetails,
                WorkOrderValue = projectList.WorkOrderValue,
                Remarks = projectList.Remarks,
                CreatedDateTime = projectList.CreatedDateTime,
                CreatedUserId = projectList.CreatedUserId,
                CreatedUserName = projectList.CreatedUserName,
                CompanyNameStr = projectList.CompanyShortName 
            };

            return View(viewModel);
        }

        // GET: ProjectLists/Create
        public IActionResult Create()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear");
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName", compId);

            return View();
        }

        // POST: ProjectLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,WorkOrderValue" +
            ",Remarks,CreatedDateTime,CreatedUserId")] ProjectList projectList)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            projectList.CompanyNameId = compId;

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a project.");
                    return View(projectList);
                }
               
                projectList.CreatedUserId = userId;
                projectList.CreatedDateTime = DateTime.Now;

                _context.Add(projectList);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", projectList.FyYear);
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            return View(projectList);
        }

        // GET: ProjectLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var projectList = await _context.ProjectList.FirstOrDefaultAsync(p => p.Id == id && p.CompanyNameId == compId);
            if (projectList == null) return NotFound();

            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", projectList.FyYear);
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            return View(projectList);
        }

        // POST: ProjectLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,WorkOrderValue,Remarks")] ProjectList projectList)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            projectList.CompanyNameId = compId;

            if (id != projectList.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProjectList = await _context.ProjectList.FirstOrDefaultAsync(p => p.Id == id && p.CompanyNameId == compId);
                if (existingProjectList == null) return NotFound();

                try
                {
                    projectList.CreatedUserId = existingProjectList.CreatedUserId;
                    projectList.CreatedDateTime = existingProjectList.CreatedDateTime;

                    _context.Entry(existingProjectList).CurrentValues.SetValues(projectList);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Project updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectListExists(projectList.Id, compId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", projectList.FyYear);
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            return View(projectList);
        }


        // GET: ProjectLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var projectList = await _context.ProjectList
                .Include(p => p.FinancialYear)
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);
            if (projectList == null) return NotFound();

            return View(projectList);
        }

        // POST: ProjectLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var projectList = await _context.ProjectList.FirstOrDefaultAsync(p => p.Id == id && p.CompanyNameId == compId);
            if (projectList != null)
            {
                _context.ProjectList.Remove(projectList);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectListExists(int id, int companyId)
        {
            return _context.ProjectList.Any(e => e.Id == id && e.CompanyNameId == companyId);
        }
    }
}
