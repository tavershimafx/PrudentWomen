using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Org.BouncyCastle.Utilities;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class LoanDisbursementController : MvcBaseController
    {
        private readonly IRepository<string, LoanDisbursement> _disbursementRepo;
        private readonly IBankManager _bankManager;

        public LoanDisbursementController(IRepository<string, LoanDisbursement> disbursementRepo)
        {
            _disbursementRepo = disbursementRepo;
        }

        public IActionResult Index(string search, DateTime? from, DateTime? to, int page = 1, int size = 50)
        {
            var disbursements = _disbursementRepo.AsQueryable()
                .Include(k => k.Loan.UserAccount.User)
                .OrderByDescending(n => n.DateCreated).Select(n => new LoanDisbursementList
                {
                    Amount = n.Amount,
                    DisbursementAccount = n.DisbursementAccount,
                    Status = n.Status,
                    DateDisbursed = n.DateDisbursed,
                    UserAccount = $"{n.Loan.UserAccount.User.FirstName} {n.Loan.UserAccount.User.LastName}",
                    UserName = n.Loan.UserAccount.User.UserName
                });

            if (!string.IsNullOrEmpty(search))
            {
                ViewData["Search"] = search;
                disbursements = disbursements.Where(c => c.UserName.Contains(search) || c.UserAccount.Contains(search));
            }

            if (from.HasValue)
            {
                ViewData["From"] = from;
                disbursements = disbursements.Where(n => n.DateDisbursed >= from);
            }

            if (to.HasValue)
            {
                ViewData["To"] = to;
                disbursements = disbursements.Where(n => n.DateDisbursed <= to);
            }

            var model = new PaginatedTable<LoanDisbursementList>()
            {
                TotalItems = disbursements.Count()
            };

            model.Data = disbursements.Skip((size * page) - size).Take(size);

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