using KLENZ.Models;
using KLENZ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;

namespace KLENZ.Controllers
{
    [Authorize] // Ensure only authenticated users can access this controller
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICompanyContext _companyContext;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ICompanyContext companyContext)
        {
            _logger = logger;
            _configuration = configuration;
            _companyContext = companyContext;
        }

        public IActionResult Index()
        {
            if (!_companyContext.HasSelectedCompany)
            {
                return RedirectToAction("Index", "CompanySelection");
            }

            int companyId = _companyContext.CurrentCompanyId!.Value;
            ViewBag.CompanyName = _companyContext.CurrentCompanyName;
            ViewBag.CompanyShortName = _companyContext.CurrentCompanyShortName;
            ViewBag.TotEnquiry = GetTotalEnquiries(companyId);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public int GetTotalEnquiries(int companyId)
        {
            int count = 0;
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string 'KLENZDbContext' is not configured.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT COUNT(*) FROM Sales.SalesEnquiry s
                                 INNER JOIN Services.FinancialYear fy ON fy.Id = s.FyYear 
                                 WHERE fy.IsActive = 1 AND (fy.CompanyNameId = @CompanyId OR fy.CompanyNameId IS NULL) AND s.CompanyNameId = @CompanyId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    conn.Open();
                    object? result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }

            return count;
        }
    }
}
