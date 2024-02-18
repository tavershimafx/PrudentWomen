using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;

namespace PrudentWomen.Core.Areas.Core.Controllers
{
    [Area("Core")]
    [Authorize(Roles = "Customer")]
    public class UserDashboardController : MvcBaseController
    {
        private readonly ILogger<RegCodeController> _logger;
        private IRepository<string, UserRegCode> _regCodeRepo;
        private int _pageSize = 50;

        public UserDashboardController(ILogger<RegCodeController> logger, IRepository<string, UserRegCode> regCodeRepo)
        {
            _logger = logger;
            _regCodeRepo = regCodeRepo;
        }
    }
}