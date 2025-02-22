using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KLENZ.Data;
using KLENZ.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLENZ.Controllers
{
    public class SalesEnquiriesController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // ✅ Fixed missing declaration

        public SalesEnquiriesController(KLENZDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment; // ✅ Initialize host environment
        }

        // GET: SalesEnquiries
        public async Task<IActionResult> Index()
        {
            return View(await _context.SalesEnquiries.ToListAsync()); // ✅ Fixed DbSet name
        }

        // GET: SalesEnquiries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SalesEnquiries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,CompanyName,ReferedBy,EnquiryDetails,EnquiryDate,CustomerDetails,Status,Remarks,RemainderDate,RemainderPlace")] SalesEnquiry salesEnquiry,
            IFormFile? File)
        {
            if (ModelState.IsValid)
            {
                if (File != null && File.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await File.CopyToAsync(fileStream);
                    }

                    salesEnquiry.FilePath = "/uploads/" + uniqueFileName; // Store relative path
                }

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
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,CompanyName,ReferedBy,EnquiryDetails,EnquiryDate,CustomerDetails,Status,Remarks,RemainderDate,RemainderPlace,FilePath")]
            SalesEnquiry salesEnquiry)
        {
            if (id != salesEnquiry.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salesEnquiry);
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
            if (salesEnquiry != null) _context.SalesEnquiries.Remove(salesEnquiry);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SalesEnquiryExists(int id)
        {
            return _context.SalesEnquiries.Any(e => e.Id == id);
        }
    }
}
