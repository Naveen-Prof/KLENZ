using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KLENZ.Data;
using KLENZ.Models;
using KLENZ.Services;

namespace KLENZ.Controllers
{
    public class ProjectKlenzChemicalsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly ICompanyContext _companyContext;

        public ProjectKlenzChemicalsController(KLENZDbContext context, ICompanyContext companyContext)
        {
            _context = context;
            _companyContext = companyContext;
        }

        // GET: ProjectKlenzChemicals
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var list = await _context.Project_KlenzChemicals
                .Where(p => p.CompanyNameId == compId)
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .OrderByDescending(p => p.CreatedDateTime)
                .Select(p => new Project_KlenzChemicals
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

        // GET: ProjectKlenzChemicals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var item = await _context.Project_KlenzChemicals
                .Include(p => p.Company)
                .Include(p => p.FinancialYear)
                .Include(p => p.GSTType)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);

            if (item == null) return NotFound();

            return View(item);
        }

        // GET: ProjectKlenzChemicals/Create
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

        // POST: ProjectKlenzChemicals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,ProjectCost,GSTTypeId,Total,WorkDuration,Remarks,CreatedDateTime,CreatedUserId")] Project_KlenzChemicals model)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            model.CompanyNameId = compId;

            if (ModelState.IsValid)
            {
                model.CreatedDateTime = DateTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Klenz Chemicals Project created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyNameId"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName", compId);
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.CompanyNameId == compId), "Id", "FyYear", model.FyYear);
            ViewData["GSTTypeId"] = new SelectList(_context.GSTTypes, "Id", "GSTType", model.GSTTypeId);
            return View(model);
        }
    }
}
