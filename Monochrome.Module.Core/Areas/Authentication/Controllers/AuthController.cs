using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Areas.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.Services.Email;
using Monochrome.Module.Core.Models;
using Monochrome.Module.Core.DataAccess;
using MediatR;
using Monochrome.Module.Core.Events;

namespace Monochrome.Module.Core.Areas.Authentication.Controllers
{
    [Area("Authentication")]
    public class AuthController : MvcBaseController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<string, UserRegCode> _userRegCode;
        private readonly IRepository<string, User> _userRepository;
        private readonly IMediator _mediator;

        public AuthController(ILogger<AuthController> logger, UserManager<User> userManager,
            SignInManager<User> signInManager, IUserStore<User> userStore,
            IEmailSender emailSender, IRepository<string, UserRegCode> userRegCode,
            IRepository<string, User> userRepository, IMediator mediator)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _emailSender = emailSender;
            _userRegCode = userRegCode;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        public async Task<IActionResult> Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var model = new LoginViewModel()
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                var user = _userRepository.AsQueryable().FirstOrDefault(k => k.NormalizedUserName == model.Email.Normalize().ToUpperInvariant());
                if (result.Succeeded && user.Status == UserStatus.Active)
                {
                    _logger.LogInformation("User logged in.");
                    if (await _userManager.IsInRoleAsync(user, "Member"))
                    {
                        returnUrl ??= Url.Action("Index", "UserDashboard", new { area = "Admin" });
                    }
                    else
                    {
                        returnUrl ??= Url.Action("Index", "Dashboard", new { area = "Admin" });
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { ReturnUrl = returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var model = new RegisterViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (IsRegCodeValid(model.RegistrationCode))
                {
                    var user = CreateUser();
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.DOB = model.DOB;

                    while (_userRepository.AsQueryable().FirstOrDefault(k => k.UserName == user.PrudentNumber) != null)
                    {
                        user.PrudentNumber = Models.User.GenerateUserNumber();
                    }

                    await _userStore.SetUserNameAsync(user, user.PrudentNumber, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        MarkCodeAsUsed(model.RegistrationCode);
                        await _userManager.AddToRoleAsync(user, "Member");
                        _logger.LogInformation("User created a new account with password.");

                        // publish user created event for consumers to work on
                        await _mediator.Publish<UserCreated>(new UserCreated { UserId = user.Id });

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        
                        //await _userManager.ConfirmEmailAsync(user, code);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Action(
                            nameof(ConfirmEmail), "Auth",
                            values: new { area = "Authentication", userId, code, returnUrl },
                            protocol: Request.Scheme);
                        //
                        await _emailSender.SendEmailAsync(model.Email, "Please Confirm Your Email Address on Prudent Women Portal",
                            $"Welcome to the Prudent Women Portal!\r\n\r\n" +
                            $"Thank you for registering. We're excited to have you join our community!\r\n\r\n" +
                            $"To ensure that we have the correct email address on file and to activate your account, please click on the link below to confirm your email:\r\n\r\n" +
                            $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Confirmation Account</a>\r\n\r\n" +
                            $"By confirming your email, you'll gain access to personalized savings and loan accounts, as well as valuable information about our organization's activities, including the Cooperative, Food Bank, Global Godly Outreach Programme, and more.\r\n\r\n" +
                            $"If you have any questions or need assistance, feel free to reach out to us at admin@prudentwomen.org.\r\n\r\n" +
                            $"Thank you for joining us on Prudent Women Portal. We look forward to empowering and supporting you on your journey!\r\n\r\n" +
                            $"Best regards,\r\n\r\n" +
                            $"Mrs. Msurshima Comfort Chenge  \r\n" +
                            $"Founder & President  \r\n " +
                            $"Prudent Women Organisation");

                        return RedirectToAction("RegisterConfirmation", new { userNumber = user.PrudentNumber });
                        //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        //{
                        //    return RedirectToAction("RegisterConfirmation");//, new { email = model.Email, returnUrl = returnUrl }
                        //}
                        //else
                        //{
                        //    await _signInManager.SignInAsync(user, isPersistent: false);
                        //    return LocalRedirect(returnUrl);
                        //}
                    }
                    AddIdentityErrors(result.Errors);
                    return View(model);
                }

                ModelState.AddModelError("", "Invalid Registration Code.");
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult RegisterConfirmation(string userNumber)
        {
            return View(new ForgotPasswordModel { Email = userNumber });
        }

        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return View();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(
                nameof(ConfirmEmail), "Auth",
                values: new { area = "Authentication", userId, code }, protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                model.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return View();
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    "ResetPassword", "Auth", values: new { area = "Authentication", code }, protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View();
        }

        public IActionResult ForgotPasswordConfirmation() => View();

        public IActionResult ResetPassword(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var model = new ResetPasswordViewModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string code)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddIdentityErrors(result.Errors);
            return View();
        }

        public IActionResult ResetPasswordConfirmation() => View();

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return LocalRedirect("~/");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            ViewData["StatusMessage"] = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return View();
        }

        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return LocalRedirect("~/");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                ViewData["StatusMessage"] = "Error changing email.";
                return View();
            }

            // In our UI email and user name are one and the same, so when we update the email
            // we need to update the user name.
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                ViewData["StatusMessage"] = "Error changing user name.";
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);
            ViewData["StatusMessage"] = "Thank you for confirming your email change.";
            return View();
        }

        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ViewData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ViewData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ExternalLoginModel model = new()
                {
                    ReturnUrl = returnUrl,
                    ProviderDisplayName = info.ProviderDisplayName
                };

                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    model.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                }

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExternalLoginConfirmationAsync(ExternalLoginModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ViewData["ErrorMessage"] = "Error loading external login information during confirmation.";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Action(
                            nameof(ConfirmEmail), "Auth",
                            values: new { area = "Authentication", userId, code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToAction(nameof(RegisterConfirmation), new { model.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                AddIdentityErrors(result.Errors);
            }

            model.ProviderDisplayName = info.ProviderDisplayName;
            return View();
        }

        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            //ReturnUrl = returnUrl;
            //RememberMe = rememberMe;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginWith2fa(TwoFactorViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return LocalRedirect("~/");
            }
        }

        public IActionResult AccessDenied() => View();

        public IActionResult Lockout() => View();

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Models.User)}'. " +
                    $"Ensure that '{nameof(Models.User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }

        private bool IsRegCodeValid(string code)
        {
            var cd =_userRegCode.AsQueryable().FirstOrDefault(n => n.Id == code);
            return cd != null && !cd.IsUsed;
        }

        private void MarkCodeAsUsed(string code)
        {
            _userRegCode.AsQueryable().FirstOrDefault(n => n.Id == code)!.IsUsed = true;
            //usr!.IsUsed = true;
            _userRegCode.SaveChanges();
        }
    }
}
