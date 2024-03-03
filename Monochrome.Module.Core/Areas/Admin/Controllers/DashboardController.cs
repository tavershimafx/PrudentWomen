using Monochrome.Module.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Areas.ViewModels;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Models;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Monochrome.Module.Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : MvcBaseController
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<UserTransaction> _transactionRepo;

        public DashboardController(IRepository<Loan> loanRepo, IRepository<UserAccount> userAccount,
            IRepository<UserTransaction> transactionRepo)
        {
            _loanRepo = loanRepo;
            _userAccount = userAccount;
            _transactionRepo = transactionRepo;
        }

        public IActionResult Index()
        {

            var loans = _loanRepo.AsQueryable().Where(k => k.Repaid == false);
            var model = new DashboardViewModel()
            {
                LoanAmount = loans.Sum(k => k.AmountGranted),
                TotalAdmins = 2,
                Balance = _userAccount.AsQueryable().Sum(n => n.Balance),
                TotalLoans = loans.Count(),
            };
            model.Loans = loans.Where(n => n.Status == LoanApplyStatus.Pending).Select(n => new LoanList
            {
                Id = n.Id,
                AmountRequested = n.AmountRequested,
                DateApplied = n.DateApplied,
                DateApproved = n.DateApproved,
                UserAccount = n.UserAccount.User.UserName,
                Disbursed = n.DateDisbursed == null ? false : true,
                Repaid = n.Repaid,
                Status = n.Status,
                Tenure = n.Tenure
            }); ;
            model.TotalOverdue = model.LoanAmount;

            var accounts = _userAccount.AsQueryable().AsNoTracking().OrderBy(n => n.Balance);
            model.HighestBalance = accounts.FirstOrDefault()?.Balance;
            model.LowestBalance = accounts.LastOrDefault()?.Balance;

            var time = DateTime.Now.Subtract(TimeSpan.FromDays(365));
            var transactions = _transactionRepo.AsQueryable().Where(p => p.Date >= time) ;

            if (transactions.Any())
            {
                transactions = transactions.OrderBy(n => n.Date);
                model.FromOneYearDate = transactions.First().Date;
                model.MaximumDate = transactions.Last().Date;
            }

            model.Data = transactions.Select(n => new decimal[] { n.Date.ToFileTime(), n.Amount });
            return View(model);
        }

    }
}