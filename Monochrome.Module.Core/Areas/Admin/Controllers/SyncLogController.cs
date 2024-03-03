using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class SyncLogController : MvcBaseController
    {
        private readonly IRepository<SyncLog> _syncLogs;

        public SyncLogController(IRepository<SyncLog> syncLogs)
        {
            _syncLogs = syncLogs;
        }

        public IActionResult Index()
        {
            var logs = _syncLogs.AsQueryable();
            return View(logs);
        }
    }
}