using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class LoansController : MvcBaseController
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly ILoanManager _loanManager;

        public LoansController(IRepository<Loan> loanRepo, IRepository<UserAccount> userAccount,
            ILoanManager loanManager)
        {
            _loanRepo = loanRepo;
            _userAccount = userAccount;
            _loanManager = loanManager;
        }

        public IActionResult Index(string search, DateTime? from, DateTime? to, int page = 1, int size = 50)
        {
            var loans = _loanRepo.AsQueryable()
                .Include(k => k.UserAccount).ThenInclude(n => n.User)
                .OrderByDescending(k => k.DateApplied)
                .Select(n => new LoanList
                {
                    Id = n.Id,
                    AmountRequested = n.AmountRequested,
                    DateApplied = n.DateApplied,
                    DateApproved = n.DateApproved,
                    UserAccount = n.UserAccount.User.UserName,
                    Disbursed = n.DateDisbursed == null? false: true,
                    Repaid = n.Repaid,
                    Status = n.Status,
                    Tenure = n.Tenure
                }).AsEnumerable();

            if (search == "repaid")
            {
                loans = loans.Where(k => k.Repaid == true);
            }
            else if (search == "approved")
            {
                loans = loans.Where(k => k.Status == LoanApplyStatus.Approved);
            }
            else if (search == "rejected")
            {
                loans = loans.Where(k => k.Status == LoanApplyStatus.Rejected);
            }
            else if (search == "pending")
            {
                loans = loans.Where(k => k.Status == LoanApplyStatus.Pending);
            }
            else if (search == "overdue")
            {
                loans = loans.Where(k => DateTime.Now > k.DateApplied.AddDays(k.Tenure * 30));
            }

            if (from.HasValue)
            {
                loans = loans.Where(n => n.DateApplied >= from);
            }

            if (to.HasValue)
            {
                loans = loans.Where(n => n.DateApplied <= to);
            }

            var model = new PaginatedTable<LoanList>()
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

            ViewBag.LoanStatus = EnumHelper.ToDictionary(typeof(LoanApplyStatus)).Select(x => new SelectListItem { Text = x.Key.GetDisplayName(), Value = x.Value });

            return View(model);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var loan = _loanRepo.AsQueryable()
                .Include(j => j.UserAccount.User)
                .FirstOrDefault(k => k.Id == id);

            if (loan != null)
            {
                loan.BalanceAtApproval = _userAccount.AsQueryable().FirstOrDefault(k => k.Id == loan.UserAccountId).Balance;

                var details = LoanApprovalDetails.FromLoan(loan);
                details.OutstandingLoans = _loanRepo.AsQueryable().Where(k => k.UserAccountId == loan.UserAccountId && k.Repaid == false);
                return View(details);
            }

            ModelState.AddModelError("Errors", "Loan request not found");
            return View();
        }

        [HttpPost]
        public IActionResult Details(LoanApprovalForm model)
        {
            if (ModelState.IsValid)
            {
                if (model.AmountGranted == 0)
                {
                    var lns = _loanRepo.AsQueryable()
                             .Include(j => j.UserAccount.User)
                             .FirstOrDefault(k => k.Id == model.Id);
                    ModelState.AddModelError("Errors", "Amount to grant cannot be 0");
                    return View(lns);
                }

                var result = _loanManager.Approve(model, User.Identity.Name);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("Errors", result.Error);
                var ln = _loanRepo.AsQueryable()
                 .Include(j => j.UserAccount.User)
                 .FirstOrDefault(k => k.Id == model.Id);
                return View(ln);
            }

            var loan = _loanRepo.AsQueryable()
                .Include(j => j.UserAccount.User)
                .FirstOrDefault(k => k.Id == model.Id);
            return View(loan);
        }

        public IActionResult Reject(long id, string comment)
        {
            return Ok();
        }

        public IActionResult Disburse(long id)
        {
            return Ok();
        }
    }
}