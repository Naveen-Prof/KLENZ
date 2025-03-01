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
    public class QuotationReportsController : Controller
    {
        private readonly KLENZDbContext _context;

        public QuotationReportsController(KLENZDbContext context)
        {
            _context = context;
        }

        // GET: QuotationReports
        public async Task<IActionResult> Index()
        {
            return View(await _context.QuotationReport.ToListAsync());
        }

        // GET: QuotationReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotationReport = await _context.QuotationReport
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quotationReport == null)
            {
                return NotFound();
            }

            return View(quotationReport);
        }

        // GET: QuotationReports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QuotationReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuotationDate,CompanyName,ProductDetails,CustomerDetails,QuotationValue,Remarks,IsPositive,CreatedDateTime,CreatedUserId")] QuotationReport quotationReport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quotationReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quotationReport);
        }

        // GET: QuotationReports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotationReport = await _context.QuotationReport.FindAsync(id);
            if (quotationReport == null)
            {
                return NotFound();
            }
            return View(quotationReport);
        }

        // POST: QuotationReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationDate,CompanyName,ProductDetails,CustomerDetails,QuotationValue,Remarks,IsPositive,CreatedDateTime,CreatedUserId")] QuotationReport quotationReport)
        {
            if (id != quotationReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quotationReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuotationReportExists(quotationReport.Id))
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
            return View(quotationReport);
        }

        // GET: QuotationReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotationReport = await _context.QuotationReport
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quotationReport == null)
            {
                return NotFound();
            }

            return View(quotationReport);
        }

        // POST: QuotationReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quotationReport = await _context.QuotationReport.FindAsync(id);
            if (quotationReport != null)
            {
                _context.QuotationReport.Remove(quotationReport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuotationReportExists(int id)
        {
            return _context.QuotationReport.Any(e => e.Id == id);
        }
    }
}
