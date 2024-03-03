using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Microsoft.EntityFrameworkCore;
using Monochrome.Module.Core.Areas.Admin.ViewModels;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserAccountController : MvcBaseController
    {
        private readonly IRepository<UserAccount> _accountRepo;
        private int _pageSize = 50;

        public UserAccountController(IRepository<UserAccount> accountRepo)
        {
            _accountRepo = accountRepo;
        }

        public IActionResult Index(string search, int page = 1)
        {
            var transactions = _accountRepo.AsQueryable()
                .Include(n => n.User)
                .Select(k => new UserAccountList
                { 
                    Balance = k.Balance,
                    FullName = $"{k.User.FirstName} {k.User.LastName}",
                    UserName = k.User.UserName,
                    Id = k.Id
                })
                .AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                ViewData["Search"] = search;
                transactions = transactions.Where(n => n.FullName.Contains(search) || n.UserName.Contains(search));
            }

            var model = new PaginatedTable<UserAccountList>()
            {
                TotalItems = transactions.Count()
            };

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

            return View(model);
        }        
    }
}