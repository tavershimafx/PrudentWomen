using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Customer")]
    public class UserDashboardController : MvcBaseController
    {
        private readonly IRepository<UserTransaction> _transactionRepo;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<string, User> _userRepo;

        public UserDashboardController(IRepository<UserTransaction> transactionRepo, IRepository<UserAccount> userAccount,
            IRepository<string, User> userRepo, IRepository<Loan> loanRepo)
        {
            _transactionRepo = transactionRepo;
            _userAccount = userAccount;
            _userRepo = userRepo;
            _loanRepo = loanRepo;
        }

        public IActionResult Index(int page = 1, int size = 50)
        {
            var user = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == User.Identity.Name);
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
            var transactions = _transactionRepo.AsQueryable()
                .Where(n => n.UserAccountId == account.Id)
                .OrderByDescending(k => k.Date);

            var model = new PaginatedTable<UserTransaction>()
            {
                TotalItems = transactions.Count()
            };

            ViewData["Balance"] = (account.Balance/100).ToString("N2");

            var loans = _loanRepo.AsQueryable().Where(n => n.UserAccountId == account.Id && n.Status == ApplicationStatus.Approved);
            var total = loans.Sum(n => n.AmountGranted);
            ViewData["Loan"] = (total/100).ToString("N2");

            model.Data = transactions.Skip((size * page) - size).Take(size);

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
    }
}