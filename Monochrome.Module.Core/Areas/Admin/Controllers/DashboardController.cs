using Monochrome.Module.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
            var allTransactions = _transactionRepo.AsQueryable();
            var loans = _loanRepo.AsQueryable().Where(k => k.Repaid == false);
            var accounts = _userAccount.AsQueryable()
                .Include(k => k.User).AsNoTracking().OrderByDescending(n => n.Balance);

            var credit = allTransactions.Where(k => k.Type == "credit").Sum(n => n.Amount);
            var debit = allTransactions.Where(k => k.Type == "debit").Sum(n => n.Amount);
            var unpaidLoans = loans.Where(n => n.Status == ApplicationStatus.Approved && n.Repaid == false).Sum(k => k.AmountGranted);

            var model = new DashboardViewModel()
            {
                UnPaidLoans = unpaidLoans,
                TotalMembers = accounts.Count(),
                Balance = credit - debit - unpaidLoans,
                TotalLoans = loans.Count(),
            };

            model.PendingLoans = loans.Where(n => n.Status == ApplicationStatus.Pending).Select(n => new LoanList
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
            model.TotalOverdue = unpaidLoans;


            model.HighestBalance = accounts.FirstOrDefault()?.Balance;
            model.HighestBalanceUserName = accounts.FirstOrDefault()?.User.UserName;
            model.LowestBalance = accounts.LastOrDefault()?.Balance;
            model.LowestBalanceUserName = accounts.LastOrDefault()?.User.UserName;

            var time = DateTime.Now.Subtract(TimeSpan.FromDays(400));
            var analytics = allTransactions.Where(p => p.Date >= time)
                .GroupBy(k => k.Date.Date)
                .Select(n => new UserTransaction
                {
                    Amount = n.Sum(p => p.Amount),
                    Date = n.First().Date
                });

            if (analytics.Any())
            {
                analytics = analytics.OrderBy(n => n.Date);
                model.FromOneYearDate = analytics.First().Date;
                model.MaximumDate = analytics.Last().Date;
            }

            model.Data = analytics.Select(n => new object[] { n.Date.ToUnixTimeMilliseconds(), n.Amount/100 });
            return View(model);
        }

    }
}