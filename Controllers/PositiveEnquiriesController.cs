using KLENZ.Data;
using KLENZ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KLENZ.Controllers
{
    public class PositiveEnquiriesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public PositiveEnquiriesController(KLENZDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public string GetActiveFinancialYear()
        {
            string financialYear = "";
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'KLENZDbContext' is not configured.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT FyYear, Id FROM Services.FinancialYear WHERE IsActive = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        financialYear = $"{reader.GetString(0)}";
                    }
                }
            }
            return financialYear;
        }
        public int GetFyId()
        {
            int fyId = 0;
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id FROM Services.FinancialYear WHERE IsActive = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        fyId = reader.GetInt32(0);
                    }
                }
            }

            return fyId;
        }

        // GET: PositiveEnquiries
        public async Task<IActionResult> Index()
        {
            var companyNameList = await _context.PositiveEnquiry.Include(c => c.Company).OrderByDescending(c => c.CreatedDateTime).ToListAsync();

            if (companyNameList == null)
            {
                return View(new List<PositiveEnquiry>());
            }

            return View(companyNameList);
        }

        // GET: PositiveEnquiries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var positiveEnquiry = await _context.PositiveEnquiry
                .Where(m => m.Id == id)
                .Select(pe => new
                {
                    pe.Id,
                    pe.QuotationDate,
                    pe.ProductDetails,
                    pe.CustomerDetails,
                    pe.QuotationValue,
                    pe.CurrentStatus,
                    pe.CreatedDateTime,
                    pe.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == pe.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    CompanyName = _context.CompanyName
                        .Where(c => c.Id == pe.CompanyNameId) // Assuming CompanyNameId is the foreign key
                        .Select(c => c.ShortName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (positiveEnquiry == null) return NotFound();

            var viewModel = new PositiveEnquiry
            {
                Id = positiveEnquiry.Id,
                QuotationDate = positiveEnquiry.QuotationDate,
                ProductDetails = positiveEnquiry.ProductDetails,
                CustomerDetails = positiveEnquiry.CustomerDetails,
                QuotationValue = positiveEnquiry.QuotationValue,
                CurrentStatus = positiveEnquiry.CurrentStatus,
                CreatedDateTime = positiveEnquiry.CreatedDateTime,
                CreatedUserId = positiveEnquiry.CreatedUserId,
                CreatedUserName = positiveEnquiry.CreatedUserName,
                CompanyNameStr = positiveEnquiry.CompanyName
            };


            return View(viewModel);
        }


        // GET: PositiveEnquiries/Create
        public IActionResult Create()
        {
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName");
            ViewBag.ActiveFinancialYear = GetActiveFinancialYear();
            return View();
        }

        // POST: PositiveEnquiries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuotationDate,CompanyNameId,ProductDetails,CustomerDetails,QuotationValue," +
            "CurrentStatus,CreatedDateTime,CreatedUserId")] PositiveEnquiry positiveEnquiry)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a sales enquiry.");
                    return View(positiveEnquiry);
                }
                positiveEnquiry.CreatedUserId = userId;
                positiveEnquiry.CreatedDateTime = DateTime.Now;
                positiveEnquiry.FyYear = GetFyId();

                _context.Add(positiveEnquiry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName");
            return View(positiveEnquiry);
        }

        // GET: PositiveEnquiries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var positiveEnquiry = await _context.PositiveEnquiry.FindAsync(id);
            if (positiveEnquiry == null)
            {
                return NotFound();
            }

            ViewBag.Companies = _context.CompanyName
              .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
              .ToList();
            return View(positiveEnquiry);
        }

        // POST: PositiveEnquiries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationDate,CompanyNameId,ProductDetails,CustomerDetails" +
            ",QuotationValue,CurrentStatus,CreatedDateTime,CreatedUserId")] PositiveEnquiry positiveEnquiry)
        {
            if (id != positiveEnquiry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPosEnquiry = await _context.SalesEnquiries.FindAsync(id);
                    if (existingPosEnquiry == null) return NotFound();

                    positiveEnquiry.CreatedUserId = existingPosEnquiry.CreatedUserId;
                    positiveEnquiry.CreatedDateTime = existingPosEnquiry.CreatedDateTime;
                    positiveEnquiry.FyYear = GetFyId();

                    _context.Entry(existingPosEnquiry).CurrentValues.SetValues(positiveEnquiry);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositiveEnquiryExists(positiveEnquiry.Id))
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

            ViewBag.Companies = _context.CompanyName
               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
               .ToList();

            return View(positiveEnquiry);
        }

        // GET: PositiveEnquiries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var positiveEnquiry = await _context.PositiveEnquiry
                .FirstOrDefaultAsync(m => m.Id == id);
            if (positiveEnquiry == null)
            {
                return NotFound();
            }

            return View(positiveEnquiry);
        }

        // POST: PositiveEnquiries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var positiveEnquiry = await _context.PositiveEnquiry.FindAsync(id);
            if (positiveEnquiry != null)
            {
                _context.PositiveEnquiry.Remove(positiveEnquiry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PositiveEnquiryExists(int id)
        {
            return _context.PositiveEnquiry.Any(e => e.Id == id);
        }
    }
}
