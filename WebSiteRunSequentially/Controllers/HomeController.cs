﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebSiteRunSequentially.Database;
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

        public IActionResult Index([FromServices] WebSiteDbContext context)
        {
            var common = context.CommonNameDateTimes.SingleOrDefault();
            var logs = context.NameDateTimes.OrderByDescending(x => x.DateTimeUtc).ToList();

            return View(new CommonLogsDto(common, logs));
        }

        public IActionResult DelLogs([FromServices] WebSiteDbContext context)
        {
            context.RemoveRange(context.NameDateTimes);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("The Privacy link has been clicked");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}