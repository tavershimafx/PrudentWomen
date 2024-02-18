using Monochrome.Module.Core.Extensions;
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
                var user = new User { Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
                await _userStore.SetUserNameAsync(user, user.PrudentNumber, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action(
                        nameof(Authentication.Controllers.AuthController.ConfirmEmail), "Auth",
                        values: new { area = "Authentication", userId, code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    var newRoles = _roleRepository.AsQueryable().Where(n => model.RoleIds.Any(k => k == n.Id));
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
        public IActionResult Index()
        {
            var users = _userRepository.AsQueryable()
                .Include(k => k.UserRoles).ThenInclude(x => x.Role)
                .OrderByDescending(k => k.DateCreated)
                .Select(x => new UserListItemVm
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = $"{x.FirstName} {x.LastName}",
                    EmailConfirmed = x.EmailConfirmed,
                    Username = x.UserName,
                    DateCreated = x.DateCreated,
                    Roles = x.UserRoles.Select(n => n.Role.Name).ToArray()
                });

            ViewBag.Roles = _roleRepository.AsQueryable()
                .Where(x => x.NormalizedName != "SuperAdmin".Normalize().ToUpperInvariant())
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id });
            return View(users);
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

        [HttpGet("user-status")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult UserStatusEnums()
        {
            var model = EnumHelper.ToDictionary(typeof(UserStatus)).Select(x => new { Id = x.Key.ToString(), Name = x.Value });
            return Ok(model);
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