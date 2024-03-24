using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Monochrome.Module.Core.Areas.Admin.ViewModels;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserTransactionController : MvcBaseController
    {
        private readonly IRepository<UserTransaction> _transactionRepo;
        private int _pageSize = 50;

        public UserTransactionController(IRepository<UserTransaction> transactionRepo)
        {
            _transactionRepo = transactionRepo;
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
                var tranStatement = transactions.Select(k => new TransactionStatement
                {
                    Amount = k.Amount,
                    Date = k.Date,
                    Type = k.Type
                });
                Response.Headers.Add("Content-Disposition", $"attachment; filename={transactions.First().UserAccount.User.UserName}statement.csv");
                var by = Encoding.UTF8.GetBytes(CsvConverter.ExportCsv(tranStatement));
                return new FileContentResult(by, "text/csv");
            }

            return View(model);
        }

        public IActionResult DownloadStatement(long accountId, DateTime? from, DateTime? to)
        {
            return Ok();
        }
    }
}