using KLENZ.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace KLENZ.Controllers
{
    [Authorize] // Ensure only authenticated users can access this controller
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.TotEnquiry = GetTotalEnquiries();
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

        public int GetTotalEnquiries()
        {
            int count = 0;
            string? connectionString = _configuration.GetConnectionString("KLENZDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string 'KLENZDbContext' is not configured.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Sales.SalesEnquiry s\r\nINNER JOIN Services.FinancialYear fy ON fy.Id = s.FyYear \r\nWHERE fy.IsActive = 1\r\n";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                count = (int)cmd.ExecuteScalar();
            }

            return count;
        }

    }
}
