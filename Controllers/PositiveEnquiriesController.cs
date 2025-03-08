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
    public class PositiveEnquiriesController : Controller
    {
        private readonly KLENZDbContext _context;

        public PositiveEnquiriesController(KLENZDbContext context)
        {
            _context = context;
        }

        // GET: PositiveEnquiries
        public async Task<IActionResult> Index()
        {
            return View(await _context.PositiveEnquiry.ToListAsync());
        }

        // GET: PositiveEnquiries/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: PositiveEnquiries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PositiveEnquiries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuotationDate,CompanyName,ProductDetails,CustomerDetails,QuotationValue,CurrentStatus,CreatedDateTime,CreatedUserId")] PositiveEnquiry positiveEnquiry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(positiveEnquiry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            return View(positiveEnquiry);
        }

        // POST: PositiveEnquiries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationDate,CompanyName,ProductDetails,CustomerDetails,QuotationValue,CurrentStatus,CreatedDateTime,CreatedUserId")] PositiveEnquiry positiveEnquiry)
        {
            if (id != positiveEnquiry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(positiveEnquiry);
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
