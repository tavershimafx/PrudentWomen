using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Customer")]
    public class UserDashboardController : MvcBaseController
    {
        private readonly ILogger<UserDashboardController> _logger;
        private IRepository<string, UserRegCode> _regCodeRepo;
        private int _pageSize = 50;

        public UserDashboardController(ILogger<UserDashboardController> logger, IRepository<string, UserRegCode> regCodeRepo)
        {
            _logger = logger;
            _regCodeRepo = regCodeRepo;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}