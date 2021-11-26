using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Test.EfCore;
using WebSiteRunSequentially.Models;

namespace WebSiteRunSequentially.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index([FromServices] TestDbContext context)
        {
            var common = context.CommonNameDateTimes.SingleOrDefault();
            var logs = context.NameDateTimes.OrderBy(x => x.DateTimeUtc).ToList();

            return View(new CommonLogsDto(common, logs));
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
    }
}