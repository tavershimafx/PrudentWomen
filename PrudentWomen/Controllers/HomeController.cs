using Microsoft.AspNetCore.Mvc;
using PrudentWomen.Models;
using Monochrome.Module.Core.Services;
using System.Diagnostics;

namespace PrudentWomen.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBankManager _bankManager;

        public HomeController(ILogger<HomeController> logger, IBankManager bankManager)
        {
            _logger = logger;
            _bankManager = bankManager;
        }

        public async Task<IActionResult> Index()
        {
            await _bankManager.FetchTransactions();
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
    }
}