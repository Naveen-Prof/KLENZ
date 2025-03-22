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
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectListsController(KLENZDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ProjectLists
        public async Task<IActionResult> Index()
        {
            var projectLists = await _context.ProjectList
                .Include(p => p.FinancialYear) // Ensure FinancialYear is loaded
                .Include(c => c.Company)
                .ToListAsync();

            if (projectLists == null)
            {
                return View(new List<ProjectList>()); // Ensure Model is never null
            }

            return View(projectLists);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var projectList = await _context.ProjectList
                .Include(p => p.FinancialYear)  // Ensure FinancialYear is loaded
                .Include(p => p.Company)        // Ensure Company entity is loaded
                .Where(p => p.Id == id)
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
                    CompanyShortName = p.Company != null ? p.Company.ShortName : "N/A", // Fetch Company Name
                    FinancialYear = _context.FinancialYear
                        .Where(f=> f.Id == p.FyYear)
                        .Select(f => f.FyYear)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (projectList == null) return NotFound();

            var viewModel = new ProjectList
            {
                Id = projectList.Id,
                FinancialYearStr = projectList.FinancialYear, // Convert if needed
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
            ViewData["FyYear"] = new SelectList(_context.FinancialYear.Where(fy => fy.IsActive == 1),"Id", "FyYear");
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(c => c.IsActive == 1), "Id", "ShortName");

            return View();
        }

        // POST: ProjectLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,WorkOrderValue" +
            ",Remarks,CreatedDateTime,CreatedUserId")] ProjectList projectList)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a sales enquiry.");
                    return View(projectList);
                }
               
                // Set the CreatedUserId and CreatedDateTime
                projectList.CreatedUserId = userId;
                projectList.CreatedDateTime = DateTime.Now;

                _context.Add(projectList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", projectList.FyYear);
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName");
            return View(projectList);
        }

        // GET: ProjectLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectList = await _context.ProjectList.FindAsync(id);
            if (projectList == null)
            {
                return NotFound();
            }
            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", projectList.FyYear);
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName");
            return View(projectList);
        }

        // POST: ProjectLists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FyYear,WorkOrderDate,CompanyNameId,CustomerDetails,WorkDetails,WorkOrderValue,Remarks")] ProjectList projectList)
        {
            if (id != projectList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProjectList = await _context.ProjectList.FindAsync(id);
                if (existingProjectList == null)
                {
                    return NotFound();
                }

                try
                {
                    // Preserve CreatedUserId and CreatedDateTime
                    projectList.CreatedUserId = existingProjectList.CreatedUserId;
                    projectList.CreatedDateTime = existingProjectList.CreatedDateTime;

                    // Update only the modified fields
                    _context.Entry(existingProjectList).CurrentValues.SetValues(projectList);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectListExists(projectList.Id))
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

            ViewData["FyYear"] = new SelectList(_context.FinancialYear, "Id", "FyYear", projectList.FyYear);
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName");
            return View(projectList);
        }


        // GET: ProjectLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectList = await _context.ProjectList
                .Include(p => p.FinancialYear)
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectList == null)
            {
                return NotFound();
            }

            return View(projectList);
        }

        // POST: ProjectLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectList = await _context.ProjectList.FindAsync(id);
            if (projectList != null)
            {
                _context.ProjectList.Remove(projectList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectListExists(int id)
        {
            return _context.ProjectList.Any(e => e.Id == id);
        }
    }
}
