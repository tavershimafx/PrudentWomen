using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Member")]
    public class UserLoansController : MvcBaseController
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<LoanRepaymentHistory> _repayHistory;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<string, User> _userRepo;
        private readonly ILoanManager _loanManager;
        private readonly IBankManager _bankManager;

        public UserLoansController(IRepository<Loan> loanRepo, IRepository<UserAccount> userAccount,
            IRepository<string, User> userRepo, ILoanManager loanManager, IBankManager bankManager,
            IRepository<LoanRepaymentHistory> repayHistory)
        {
            _loanRepo = loanRepo;
            _userAccount = userAccount;
            _userRepo = userRepo;
            _loanManager = loanManager;
            _bankManager = bankManager;
            _repayHistory = repayHistory;
        }

        public IActionResult Index(int page = 1, int size = 50)
        {
            var user = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == User.Identity.Name);
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
            var loans = _loanRepo.AsQueryable()
                .Where(n => n.UserAccountId == account.Id)
                .OrderByDescending(k => k.DateApplied);

            var model = new PaginatedTable<Loan>()
            {
                TotalItems = loans.Count()
            };

            model.Data = loans.Skip((size * page) - size).Take(size);

            model.PageSize = size;
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / size);
            model.Page = page;
            if (model.TotalPages > 1)
            {
                var currentPage = page;
                var totalPages = model.TotalPages;
                List<int> pages = new List<int>();
                if (page == 1)
                {
                    // we have to paginate forward
                    var i = currentPage;
                    while (i < totalPages)
                    {
                        // we want to show maximum of 5 pagination buttons
                        if (pages.Count >= 5)
                        {
                            break;
                        }

                        pages.Add(i + 1);
                        i++;
                    }
                }
                else if (currentPage == totalPages)
                {
                    // we have to paginate backward
                    var i = currentPage;
                    while (i > 1)
                    {
                        // we want to show maximum of 5 pagination buttons
                        if (pages.Count >= 5)
                        {
                            break;
                        }

                        pages.Add(i - 1);
                        i--;
                    }
                }
                else
                {
                    // paginate max two forward and max two backward

                    // start with backward
                    var i = currentPage;
                    while (i < (currentPage - 3) && i > 1)
                    {
                        if ((currentPage - 3) == 1)
                        {
                            break;
                        }

                        pages.Add(i - 1);
                        i--;
                    }

                    //add current page
                    pages.Add(currentPage);

                    //paginate max two forward
                    var x = currentPage;
                    while (x < totalPages)
                    {
                        // at this point, we assume there are less than five items to paginate
                        if (pages.Count >= 5)
                        {
                            break;
                        }

                        pages.Add(x + 1);
                        x++;
                    }
                }

                model.Pages = pages.ToArray();
            }

            return View(model);
        }

        public async Task<IActionResult> Apply()
        {
            ViewData["Banks"] = (await _bankManager.FetchBanks()).Data.Select(n => new SelectListItem() { Text = n.Name, Value = n.Nip_code});
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Apply(LoanApplyForm model)
        {
            if (ModelState.IsValid)
            {
                var result = await _loanManager.Apply(model, User.Identity.Name);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                ViewData["Banks"] = (await _bankManager.FetchBanks()).Data.Select(n => new SelectListItem() { Text = n.Name, Value = n.Nip_code });
                ModelState.AddModelError("Errors", result.Error);
                return View(model);
            }

            ViewData["Banks"] = (await _bankManager.FetchBanks()).Data.Select(n => new SelectListItem() { Text = n.Name, Value = n.Nip_code });
            return View(model);
        }

        public async Task<IActionResult> NameInquiry(string account, string bankCode)
        {
            AccountLookupObject lookup = new()
            {
                account_number = account,
                nip_code = bankCode
            };
            var result = await _bankManager.AccountLookup(lookup);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }

            ModelState.AddModelError("Errors", result.Error);
            return BadRequest(ModelState);
        }

        public IActionResult RepayHistory(long id, int page = 1, int size = 50)
        {
            var user = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == User.Identity.Name);
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
            var loans = _repayHistory.AsQueryable()
                .Include(p => p.Loan)
                .Where(n => n.LoanId == id && n.Loan.UserAccountId == account.Id)
                .OrderByDescending(k => k.DateCreated);

            var model = new PaginatedTable<LoanRepaymentHistory>()
            {
                TotalItems = loans.Count()
            };

            model.Data = loans.Skip((size * page) - size).Take(size);

            model.PageSize = size;
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / size);
            model.Page = page;

            if (loans != null && loans.Any())
            {
                ViewData["AmountGranted"] = loans.First().Loan.AmountGranted;
                ViewData["PecentInterest"] = loans.First().Loan.PecentInterest;
            }

            return View(model);
        }
    }
}