﻿using Monochrome.Module.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Models;
using Microsoft.EntityFrameworkCore;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Monochrome.Module.Core.Services.Email;
using Microsoft.AspNetCore.Mvc.Rendering;
using Monochrome.Module.Core.Services;

namespace Monochrome.Module.Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/users")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : MvcBaseController
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<string, Role> _roleRepository;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private int _pageSize = 50;

        public UsersController(ILogger<UsersController> logger, IRepository<string, User> userRepository,
            IRepository<string, Role> roleRepository, UserManager<User> userManager,
            IEmailSender emailSender, IUserStore<User> userStore)
        {
            _logger = logger;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _emailSender = emailSender;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        [HttpGet("get-user/{id}")]
        public IActionResult GetUser(string id)
        {
            var user = _userRepository.AsQueryable()
                .Include(k => k.UserRoles).ThenInclude(x => x.Role)
                .FirstOrDefault(k => k.Id == id);
            if (user != null)
            {
                var model = new UserCreateForm
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ConcurrencyStamp = user.ConcurrencyStamp,
                    Email = user.Email,
                    UserName = user.UserName,
                    RoleIds = user.UserRoles.Select(k => k.RoleId).ToArray()
                };

                return Ok(model);
            }

            ModelState.AddModelError("Errors", "User not found.");
            return BadRequest(ModelState);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromQuery] UserCreateForm model)
        {
            if (ModelState.IsValid)
            {
                var user = new User 
                { 
                    Email = model.Email,
                    NormalizedEmail = model.Email.Normalize().ToUpperInvariant(),
                    FirstName = model.FirstName, 
                    LastName = model.LastName,
                    UserName = model.UserName,
                    NormalizedUserName = model.UserName.Normalize().ToUpperInvariant()
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    
                    //var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Action(
                            nameof(Authentication.Controllers.AuthController.ConfirmEmail), "Auth",
                            values: new { area = "Authentication", userId = user.Id, code },
                            protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                    $"<!doctype html>" +
                    $"<html lang='en'><head> <meta charset='utf-8'>" +
                    $"<title>Prudent Women Organisation</title>" +
                    $"<base href='/'>" +
                    $"<meta name='viewport' content='width=device-width, initial-scale=1'>" +
                    $"<link rel='icon' type='image/x-icon' href=''>" +
                    $"</head><body>" +
                    $"<h1>Please Confirm Your Email Address on Prudent Women Portal</h1>" +
                    $"<p>You have just changed your email address on your Prudent Women Account. " +
                    $"By <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>, you are confirming this change.</p>" +
                    $"<p>Ignore this message if you did not initiate this change, or if this message was sent to you in error.</p>" +
                    $"<p>You can report any suspicious activity on your account by visiting our " +
                    $"website <a href='www.prudentwomen.org'>www.prudentwomen.org</a> and contacting the customer care officers, " +
                    $"or call us directly via our Help Desk Line: +234 703 602 5402</p>" +
                    $"</body></html>");

                    var newRoles = _roleRepository.AsQueryable().QueryInChunksOf(10)
                        .Where(n => model.RoleIds.Any(k => k == n.Id));

                    if (newRoles != null && newRoles.Any())
                    {
                        foreach (var role in newRoles)
                        {
                            await _userManager.AddToRoleAsync(user, role.Name);
                        }
                    }
                    return Ok();
                }

                AddIdentityErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromQuery] UserUpdateForm model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.AsQueryable()
                .Include(k => k.UserRoles).ThenInclude(x => x.Role)
                .FirstOrDefault(k => k.Id == model.Id);

                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.UserName;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    await _userManager.UpdateNormalizedUserNameAsync(user);
                    await _userManager.UpdateNormalizedEmailAsync(user);

                    if (!user.UserRoles.Any(k => k.Role.NormalizedName == "SuperAdmin".Normalize().ToUpperInvariant()))
                    {
                        if (model.RoleIds != null && model.RoleIds.Any())
                        {
                            //var newRoles = _roleRepository.AsQueryable().Where(n => model.RoleIds.Any(k => k == n.Id));
                            //await _userManager.AddToRolesAsync(user, newRoles.Select(n => n.Name));
                            user.UserRoles = user.UserRoles.Where(k => model.RoleIds.Any(n => n == k.RoleId)).ToList();
                            var newRoleIds = model.RoleIds.Where(n => !user.UserRoles.Any(p => p.RoleId == n)).ToArray();
                            var newRoles = _roleRepository.AsQueryable().Where(n => newRoleIds.Any(k => k == n.Id));

                            if (newRoles != null && newRoles.Any())
                            {
                                foreach (var role in newRoles)
                                {
                                    user.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = user.Id });
                                }
                            }
                        }
                        else
                        {
                            user.UserRoles = null;
                        }
                    }
                    else
                    {
                        // we don't want to update the username for SuperAdmin user to something else
                        user.UserName = "SuperAdmin";
                        await _userManager.UpdateNormalizedUserNameAsync(user);
                    }

                    _userRepository.SaveChanges();

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        await _userManager.RemovePasswordAsync(user);
                        await _userManager.AddPasswordAsync(user, model.Password);
                    }

                    return Ok("User updated");
                }

                ModelState.AddModelError("Errors", "User not found.");
                return BadRequest(ModelState);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("list")]
        public IActionResult Index(string search, int page = 1)
        {
            var users = _userRepository.AsQueryable()
                .Include(k => k.UserRoles).ThenInclude(x => x.Role).AsQueryable();
                
            if (!string.IsNullOrEmpty(search))
            {
                ViewData["Search"] = search;
                users = users.Where(n => n.Email.Contains(search) || n.UserName.Contains(search) ||
                n.FirstName.Contains(search) || n.LastName.Contains(search));
            }

            var searched = users.Select(x => new UserListItemVm
            {
                Id = x.Id,
                Email = x.Email,
                FullName = $"{x.FirstName} {x.LastName}",
                EmailConfirmed = x.EmailConfirmed,
                Username = x.UserName,
                DateCreated = x.DateCreated,
                Roles = x.UserRoles.Select(n => n.Role.Name).ToArray(),
                Status = x.Status
            }).OrderByDescending(k => k.DateCreated);

            ViewBag.Roles = _roleRepository.AsQueryable()
                .Where(x => x.NormalizedName != "SuperAdmin".Normalize().ToUpperInvariant())
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id });

            ViewBag.Status = EnumHelper.ToDictionary(typeof(UserStatus)).Select(x => x.Value);
            var model = new PaginatedTable<UserListItemVm>()
            {
                TotalItems = searched.Count()
            };

            model.Data = searched.Skip((_pageSize * page) - _pageSize).Take(_pageSize);

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

        [HttpGet("change-status")]
        public IActionResult ChangeUserStatus(string userId, string status)
        {
            var user = _userRepository.AsQueryable()
                .Include(k => k.UserRoles).ThenInclude(n => n.Role)
                .FirstOrDefault(x => x.Id == userId);

            if (user != null)
            {
                if (!user.UserRoles.Any(k => k.Role.NormalizedName == "SuperAdmin".Normalize().ToUpperInvariant()))
                {
                    var parsed = Enum.TryParse(status, true, out UserStatus stat);
                    if (parsed)
                    {
                        user.Status = stat;
                        _userRepository.SaveChanges();

                        return Ok();
                    }

                    ModelState.AddModelError("Errors", "Invalid status");
                    return BadRequest(ModelState);
                }

                ModelState.AddModelError("Errors", "Cannot change the status of the specified user.");
                return BadRequest(ModelState);
            }

            ModelState.AddModelError("Errors", "User not found.");
            return BadRequest(ModelState);
        }

        [HttpDelete("delete-user")]
        public IActionResult DeleteUser(string userId)
        {
            var user = _userRepository.AsQueryable()
                .Include(k => k.UserRoles).ThenInclude(n => n.Role)
                .FirstOrDefault(x => x.Id == userId);

            if (user != null)
            {
                if (!user.UserRoles.Any(k => k.Role.NormalizedName == "SuperAdmin".Normalize().ToUpperInvariant()))
                {
                    _userRepository.Delete(user);
                    _userRepository.SaveChanges();
                    return Ok("User Deleted");
                }

                ModelState.AddModelError("Errors", "Cannot delete this user");
                return BadRequest(ModelState);
            }
            
            ModelState.AddModelError("Errors", "User not found.");
            return BadRequest(ModelState);
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}