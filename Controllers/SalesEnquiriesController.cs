using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KLENZ.Data;
using KLENZ.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
// using NuGet.Versioning;

namespace KLENZ.Controllers
{
    public class SalesEnquiriesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly KLENZ.Services.ICompanyContext _companyContext;

        public SalesEnquiriesController(KLENZDbContext context, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager, IConfiguration configuration, KLENZ.Services.ICompanyContext companyContext)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _configuration = configuration;
            _companyContext = companyContext;
        }

        public string GetActiveFinancialYear(int? companyId = null)
        {
            int compId = companyId ?? _companyContext.CurrentCompanyId ?? 0;
            string financialYear = "";
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'KLENZDbContext' is not configured.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 1 FyYear FROM Services.FinancialYear WHERE IsActive = 1 AND (CompanyNameId = @CompanyId OR (CompanyNameId IS NULL AND @CompanyId = 0)) ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", compId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            financialYear = $"{reader.GetString(0)}";
                        }
                    }
                }
            }
            return financialYear;
        }

        public int GetFyId(int? companyId = null)
        {
            int compId = companyId ?? _companyContext.CurrentCompanyId ?? 0;
            int fyId = 0;
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 1 Id FROM Services.FinancialYear WHERE IsActive = 1 AND (CompanyNameId = @CompanyId OR (CompanyNameId IS NULL AND @CompanyId = 0)) ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", compId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fyId = reader.GetInt32(0);
                        }
                    }
                }
            }

            return fyId;
        }

        // GET: SalesEnquiries
        public async Task<IActionResult> Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var salesEnquiryList = await _context.SalesEnquiries
                .Where(se => se.CompanyNameId == compId)
                .Include(se => se.Company) 
                .OrderByDescending(se => se.CreatedDateTime) 
                .Select(se => new SalesEnquiry
                {
                    Id = se.Id,
                    CompanyNameId = se.CompanyNameId,
                    FyYear = se.FyYear,
                    Company = se.Company,
                    ReferedBy = se.ReferedBy,
                    EnquiryDetails = se.EnquiryDetails,
                    EnquiryDate = se.EnquiryDate,
                    CustomerDetails = se.CustomerDetails,
                    Status = se.Status,
                    Remarks = se.Remarks,
                    ReminderDate = se.ReminderDate,
                    ReminderPlace = se.ReminderPlace,
                    FilePath = se.FilePath,
                    CreatedDateTime = se.CreatedDateTime,
                    CreatedUserId = se.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == se.CreatedUserId)
                        .Select(u => u.UserName ?? u.Email)
                        .FirstOrDefault() ?? "System"
                })
                .ToListAsync();

            return View(salesEnquiryList ?? new List<SalesEnquiry>());
        }

        // GET: SalesEnquiries/Create
        public IActionResult Create()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            ViewBag.ActiveFinancialYear = GetActiveFinancialYear(compId);
            return View();
        }

        // GET: SalesEnquiries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var salesEnquiry = await _context.SalesEnquiries
                .Where(m => m.Id == id && m.CompanyNameId == compId)
                .Select(se => new
                {
                    se.Id,
                    se.ReferedBy,
                    se.EnquiryDetails,
                    se.EnquiryDate,
                    se.CustomerDetails,
                    se.Status,
                    se.Remarks,
                    se.ReminderDate,
                    se.ReminderPlace,
                    se.FilePath,
                    se.CreatedDateTime,
                    se.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == se.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    CompanyName = _context.CompanyName
                        .Where(c => c.Id == se.CompanyNameId)
                        .Select(c => c.ShortName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (salesEnquiry == null) return NotFound();

            var viewModel = new SalesEnquiry
            {
                Id = salesEnquiry.Id,
                ReferedBy = salesEnquiry.ReferedBy,
                EnquiryDetails = salesEnquiry.EnquiryDetails,
                EnquiryDate = salesEnquiry.EnquiryDate,
                CustomerDetails = salesEnquiry.CustomerDetails,
                Status = salesEnquiry.Status,
                Remarks = salesEnquiry.Remarks,
                ReminderDate = salesEnquiry.ReminderDate,
                ReminderPlace = salesEnquiry.ReminderPlace,
                FilePath = salesEnquiry.FilePath,
                CreatedDateTime = salesEnquiry.CreatedDateTime,
                CreatedUserId = salesEnquiry.CreatedUserId,
                CreatedUserName = salesEnquiry.CreatedUserName,
                CompanyNameStr = salesEnquiry.CompanyName
            };

            return View(viewModel);
        }


        // POST: SalesEnquiries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyNameId, ReferedBy, EnquiryDetails, EnquiryDate, " +
            "CustomerDetails, Status, Remarks, ReminderDate, ReminderPlace, FilePath")] SalesEnquiry salesEnquiry, IFormFile? File)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            salesEnquiry.CompanyNameId = compId;

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    salesEnquiry.CreatedUserId = userId;
                }
                else
                {
                    salesEnquiry.CreatedUserId = null;
                }

                salesEnquiry.CreatedDateTime = DateTime.Now;

                if (File != null && File.Length > 0)
                {
                    string fileExtension = Path.GetExtension(File.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("File", "Invalid file type. Only JPG, PNG, PDF, and DOCX are allowed.");
                        return View(salesEnquiry);
                    }

                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder); 

                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await File.CopyToAsync(fileStream);
                    }

                    salesEnquiry.FilePath = "/uploads/" + uniqueFileName;
                }
                salesEnquiry.FyYear = GetFyId(compId);
                // ✅ Save Data to Database
                _context.Add(salesEnquiry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sales Enquiry created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Companies"] = new SelectList(_context.CompanyName.Where(fy => fy.IsActive == 1), "Id", "ShortName", compId);
            ViewBag.ActiveFinancialYear = GetActiveFinancialYear(compId);

            return View(salesEnquiry);
        }



        // GET: SalesEnquiries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var salesEnquiry = await _context.SalesEnquiries.FirstOrDefaultAsync(se => se.Id == id && se.CompanyNameId == compId);
            if (salesEnquiry == null) return NotFound();

            ViewBag.Companies = _context.CompanyName
               .Where(c => c.Id == compId || c.IsActive == 1)
               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
               .ToList();

            return View(salesEnquiry);
        }

        // POST: SalesEnquiries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalesEnquiry salesEnquiry, IFormFile? File)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            salesEnquiry.CompanyNameId = compId;

            if (id != salesEnquiry.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEnquiry = await _context.SalesEnquiries.FirstOrDefaultAsync(se => se.Id == id && se.CompanyNameId == compId);
                    if (existingEnquiry == null) return NotFound();

                    // Handle File Upload
                    if (File != null && File.Length > 0)
                    {
                        string fileExtension = Path.GetExtension(File.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("File", "Invalid file type. Only JPG, PNG, PDF, and DOCX are allowed.");
                            return View(salesEnquiry);
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await File.CopyToAsync(fileStream);
                        }

                        // Delete old file
                        if (!string.IsNullOrEmpty(existingEnquiry.FilePath))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingEnquiry.FilePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        salesEnquiry.FilePath = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        salesEnquiry.FilePath = existingEnquiry.FilePath;
                    }
                    salesEnquiry.CreatedUserId = existingEnquiry.CreatedUserId;
                    salesEnquiry.CreatedDateTime = existingEnquiry.CreatedDateTime;
                    salesEnquiry.FyYear = existingEnquiry.FyYear != 0 ? existingEnquiry.FyYear : GetFyId(compId);
                    _context.Entry(existingEnquiry).CurrentValues.SetValues(salesEnquiry);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sales Enquiry updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesEnquiryExists(salesEnquiry.Id, compId)) return NotFound();
                    else throw;
                }
            }

            ViewBag.Companies = _context.CompanyName
               .Where(c => c.Id == compId || c.IsActive == 1)
               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ShortName })
               .ToList();

            return View(salesEnquiry);
        }

        // GET: SalesEnquiries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            if (id == null) return NotFound();

            int compId = _companyContext.CurrentCompanyId!.Value;
            var salesEnquiry = await _context.SalesEnquiries.FirstOrDefaultAsync(m => m.Id == id && m.CompanyNameId == compId);
            if (salesEnquiry == null) return NotFound();

            return View(salesEnquiry);
        }

        // POST: SalesEnquiries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int compId = _companyContext.CurrentCompanyId!.Value;
            var salesEnquiry = await _context.SalesEnquiries.FirstOrDefaultAsync(se => se.Id == id && se.CompanyNameId == compId);
            if (salesEnquiry != null)
            {
                // Delete the associated file
                if (!string.IsNullOrEmpty(salesEnquiry.FilePath))
                {
                    string fileToDelete = Path.Combine(_hostEnvironment.WebRootPath, salesEnquiry.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fileToDelete))
                    {
                        System.IO.File.Delete(fileToDelete);
                    }
                }

                _context.SalesEnquiries.Remove(salesEnquiry);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SalesEnquiryExists(int id, int companyId)
        {
            return _context.SalesEnquiries.Any(e => e.Id == id && e.CompanyNameId == companyId);
        }
    }
}
