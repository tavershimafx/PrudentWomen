using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Monochrome.Module.Core.Areas.Admin.ViewModels;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class BankTransactionController : MvcBaseController
    {
        private readonly IRepository<string, BankTransaction> _transactionRepo;
        private readonly IBankManager _bankManager;

        public BankTransactionController(IRepository<string, BankTransaction> transactionRepo, IBankManager bankManager)
        {
            _transactionRepo = transactionRepo;
            _bankManager = bankManager;
        }

        public IActionResult Index()
        {
            var settings = _transactionRepo.AsQueryable();
            return View(settings);
        }
    }
}