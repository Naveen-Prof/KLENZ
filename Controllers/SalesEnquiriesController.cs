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
using Microsoft.EntityFrameworkCore;

namespace KLENZ.Controllers
{
    public class SalesEnquiriesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        private readonly UserManager<IdentityUser> _userManager;

        public SalesEnquiriesController(KLENZDbContext context, IWebHostEnvironment hostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        // GET: SalesEnquiries
        public async Task<IActionResult> Index()
        {
            return View(await _context.SalesEnquiries.ToListAsync());
        }

        // GET: SalesEnquiries/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: SalesEnquiries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var salesEnquiry = await _context.SalesEnquiries
                .Where(m => m.Id == id)
                .Select(se => new SalesEnquiry
                {
                    Id = se.Id,
                    CompanyName = se.CompanyName,
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
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (salesEnquiry == null) return NotFound();

            return View(salesEnquiry);
        }



        // POST: SalesEnquiries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyName, ReferedBy, EnquiryDetails, EnquiryDate, " +
            "CustomerDetails, Status, Remarks, ReminderDate, ReminderPlace, FilePath")] SalesEnquiry salesEnquiry, IFormFile? File)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a sales enquiry.");
                    return View(salesEnquiry);
                }

                salesEnquiry.CreatedUserId = userId;
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

                // ✅ Save Data to Database
                _context.Add(salesEnquiry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(salesEnquiry);
        }



        // GET: SalesEnquiries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var salesEnquiry = await _context.SalesEnquiries.FindAsync(id);
            if (salesEnquiry == null) return NotFound();

            return View(salesEnquiry);
        }

        // POST: SalesEnquiries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalesEnquiry salesEnquiry, IFormFile? File)
        {
            if (id != salesEnquiry.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEnquiry = await _context.SalesEnquiries.FindAsync(id);
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

                    _context.Entry(existingEnquiry).CurrentValues.SetValues(salesEnquiry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesEnquiryExists(salesEnquiry.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(salesEnquiry);
        }

        // GET: SalesEnquiries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var salesEnquiry = await _context.SalesEnquiries.FirstOrDefaultAsync(m => m.Id == id);
            if (salesEnquiry == null) return NotFound();

            return View(salesEnquiry);
        }

        // POST: SalesEnquiries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesEnquiry = await _context.SalesEnquiries.FindAsync(id);
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

        private bool SalesEnquiryExists(int id)
        {
            return _context.SalesEnquiries.Any(e => e.Id == id);
        }
    }
}
