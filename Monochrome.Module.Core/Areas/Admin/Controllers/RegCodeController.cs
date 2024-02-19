using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.Services;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin,SuperAdmin")]
    public class RegCodeController : MvcBaseController
    {
        private readonly ILogger<RegCodeController> _logger;
        private IRepository<string, UserRegCode> _regCodeRepo;
        private int _pageSize = 50;

        public RegCodeController(ILogger<RegCodeController> logger, IRepository<string, UserRegCode> regCodeRepo)
        {
            _logger = logger;
            _regCodeRepo = regCodeRepo;
        }

        public IActionResult Index(int page = 1)
        {
            var codes = _regCodeRepo.AsQueryable()
                .OrderBy(k => k.IsUsed)
                .Select(k => new RegCodeList
                {
                    Code = k.Id,
                    IsUsed = k.IsUsed,
                    DateUsed = k.LastUpdated.Value.ToString("dd MMM yyyy")
                });

            var model = new PaginatedTable<RegCodeList>()
            {
                TotalItems = codes.Count()
            };

            model.Data = codes.Skip((_pageSize * page) - _pageSize).Take(_pageSize);

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

        [HttpPost]
        public IActionResult Generate(int qty)
        {
            List<UserRegCode> cds = new();
            for (int i = 0; i < qty; i++)
            {
                var code = new UserRegCode();
                while ((_regCodeRepo.AsQueryable().FirstOrDefault(k => k.Id == code.Id) != null) || cds.Any(k => k.Id == code.Id))
                {
                    code.Id = UserRegCode.GenerateCode();
                }
                cds.Add(code);
            }

            _regCodeRepo.InsertRange(cds);
            _regCodeRepo.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteAll()
        {
            _regCodeRepo.DeleteRange(_regCodeRepo.AsQueryable());
            _regCodeRepo.SaveChanges();

            return Ok();
        }

        public IActionResult DownloadUnused()
        {
            var codes = _regCodeRepo.AsQueryable().Where(n => !n.IsUsed)
                .Select(k => new RegCodeList
                {
                    Code = k.Id,
                    IsUsed = k.IsUsed,
                }).AsEnumerable();

            var by = Encoding.UTF8.GetBytes(CsvConverter.ExportCsv(codes));
            Response.Headers.Add("Content-Disposition", "attachment;filename=registration-codes.csv");
            return new FileContentResult(by, "text/csv");
        }
    }
}