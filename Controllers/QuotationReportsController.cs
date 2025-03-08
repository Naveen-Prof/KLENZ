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
    public class QuotationReportsController : Controller
    {
        private readonly KLENZDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuotationReportsController(KLENZDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: QuotationReports
        public async Task<IActionResult> Index()
        {
            return View(await _context.QuotationReport.ToListAsync());
        }

        // GET: QuotationReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var quotationReport = await _context.QuotationReport
                .Where(m => m.Id == id)
                .Select(qr => new QuotationReport
                {
                    Id = qr.Id,
                    QuotationDate = qr.QuotationDate,
                    CompanyName = qr.CompanyName,
                    ProductDetails = qr.ProductDetails,
                    CustomerDetails = qr.CustomerDetails,
                    QuotationValue = qr.QuotationValue,
                    Remarks = qr.Remarks,
                    IsPositive = qr.IsPositive,
                    CreatedDateTime = qr.CreatedDateTime,
                    CreatedUserId = qr.CreatedUserId,
                    CreatedUserName = _context.Users
                        .Where(u => u.Id == qr.CreatedUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (quotationReport == null) return NotFound();

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
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "You must be logged in to create a sales enquiry.");
                    return View(quotationReport);
                }
                quotationReport.IsPositive = quotationReport.IsPositiveBool ? (byte)1 : (byte)0;

                // Set the CreatedUserId and CreatedDateTime
                quotationReport.CreatedUserId = userId;
                quotationReport.CreatedDateTime = DateTime.Now;

                // Insert into QuotationReport
                _context.Add(quotationReport);
                await _context.SaveChangesAsync();

                // If IsPositive == 1, also insert into PositiveEnquiry
                if (quotationReport.IsPositive == 1)
                {
                    var positiveEnquiry = new PositiveEnquiry
                    {
                        QuotationDate = quotationReport.QuotationDate,
                        CompanyName = quotationReport.CompanyName,
                        ProductDetails = quotationReport.ProductDetails,
                        CustomerDetails = quotationReport.CustomerDetails,
                        QuotationValue = quotationReport.QuotationValue,
                        CreatedDateTime = DateTime.Now,
                        CreatedUserId = userId
                    };

                    _context.Add(positiveEnquiry);
                    await _context.SaveChangesAsync();
                }

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
                    quotationReport.IsPositive = quotationReport.IsPositiveBool ? (byte)1 : (byte)0;
                    // Get the existing record before updating
                    var existingQuotation = await _context.QuotationReport.FindAsync(id);
                    if (existingQuotation == null)
                    {
                        return NotFound();
                    }

                    // Preserve CreatedUserId and CreatedDateTime (if needed)
                    quotationReport.CreatedUserId = existingQuotation.CreatedUserId;
                    quotationReport.CreatedDateTime = existingQuotation.CreatedDateTime;

                    // Update QuotationReport
                    _context.Entry(existingQuotation).CurrentValues.SetValues(quotationReport);
                    await _context.SaveChangesAsync();

                    // Check if IsPositive == 1 (Update or Insert into PositiveEnquiry)
                    var existingPositiveEnquiry = await _context.PositiveEnquiry.FirstOrDefaultAsync(pe => pe.Id == quotationReport.Id);
                    if (quotationReport.IsPositive == 1)
                    {
                        if (existingPositiveEnquiry == null)
                        {
                            // Insert new PositiveEnquiry
                            var positiveEnquiry = new PositiveEnquiry
                            {
                                QuotationDate = quotationReport.QuotationDate,
                                CompanyName = quotationReport.CompanyName,
                                ProductDetails = quotationReport.ProductDetails,
                                CustomerDetails = quotationReport.CustomerDetails,
                                QuotationValue = quotationReport.QuotationValue,
                                CreatedDateTime = DateTime.Now,
                                CreatedUserId = quotationReport.CreatedUserId
                            };
                            _context.Add(positiveEnquiry);
                        }
                        else
                        {
                            // Update existing PositiveEnquiry
                            existingPositiveEnquiry.QuotationDate = quotationReport.QuotationDate;
                            existingPositiveEnquiry.CompanyName = quotationReport.CompanyName;
                            existingPositiveEnquiry.ProductDetails = quotationReport.ProductDetails;
                            existingPositiveEnquiry.CustomerDetails = quotationReport.CustomerDetails;
                            existingPositiveEnquiry.QuotationValue = quotationReport.QuotationValue;
                            _context.Update(existingPositiveEnquiry);
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // If IsPositive is 0 or null, remove from PositiveEnquiry if exists
                        if (existingPositiveEnquiry != null)
                        {
                            _context.PositiveEnquiry.Remove(existingPositiveEnquiry);
                            await _context.SaveChangesAsync();
                        }
                    }
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
