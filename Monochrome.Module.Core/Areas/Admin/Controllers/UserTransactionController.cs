using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserTransactionController : MvcBaseController
    {
        private readonly IRepository<UserTransaction> _transactionRepo;
        private readonly IBankManager _bankManager;
        private readonly UserManager<User> _userManager;
        private int _pageSize = 50;

        public UserTransactionController(IRepository<UserTransaction> transactionRepo, IBankManager bankManager,
            UserManager<User> userManager)
        {
            _transactionRepo = transactionRepo;
            _bankManager = bankManager;
            _userManager = userManager;
        }

        public IActionResult Index(long accountId, DateTime? from, DateTime? to, string statement, int page = 1)
        {
            var transactions = _transactionRepo.AsQueryable()
                .Include(n => n.UserAccount.User)
                .Where(p => p.UserAccountId == accountId);

            var model = new PaginatedTable<UserTransaction>()
            {
                TotalItems = transactions.Count()
            };

            if (from.HasValue)
            {
                transactions = transactions.Where(n => n.Date >= from);
            }

            if (to.HasValue)
            {
                transactions = transactions.Where(n => n.Date <= to);
            }

            ViewData["AccountId"] = accountId;
            model.Data = transactions.Skip((_pageSize * page) - _pageSize).Take(_pageSize);

            model.PageSize = _pageSize;
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / _pageSize);
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

            if (statement == "on")
            {
                var credits = transactions.Where(k => k.Type == "credit").Select(k => new TransactionStatement
                {
                    Credit = k.Amount / 100,
                    Date = k.Date,
                    Debit = default
                });

                var debits = transactions.Where(k => k.Type == "debit").Select(k => new TransactionStatement
                {
                    Debit = k.Amount / 100,
                    Date = k.Date,
                    Credit = default
                });

                var tranStatement = debits.Concat(credits);
                Response.Headers.Add("Content-Disposition", $"attachment; filename={transactions.First().UserAccount.User.UserName}statement.csv");

                var strData = CsvConverter.ExportCsv(tranStatement);
                var cr = credits.Sum(k => k.Credit);
                var db = debits.Sum(k => k.Debit);

                strData += $",Total Credit,Total Debit\n";
                strData += $",{cr},{db}\n";
                strData += $",Balance: {cr - db},\n";

                var by = Encoding.UTF8.GetBytes(strData);
                return new FileContentResult(by, "text/csv");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SetOpeningBalance(long accountId, decimal amount)
        {
            await _bankManager.SetOpeningBalance(accountId, amount);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DebitAccount(TransferFundsModel model)
        {
            var destinationUser = await _userManager.FindByNameAsync(model.DestinationUser);
            if (destinationUser == null)
            {
                ModelState.AddModelError("Errors", "The destination user account was not found.");
                return BadRequest(ModelState);
            }

            var destinationAccount = _bankManager.GetAccount(destinationUser.Id);
            _bankManager.ExecuteTransaction(model.SourceAccountId, destinationAccount.Id, model.Amount, model.Narration);
            return Ok();
        }
    }
}