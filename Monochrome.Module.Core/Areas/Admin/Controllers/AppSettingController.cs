using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Monochrome.Module.Core.Services;
using Monochrome.Module.Core.Areas.Admin.ViewModels;

namespace Monochrome.Module.Core.Areas.Core.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AppSettingController : MvcBaseController
    {
        private readonly IRepository<string, ApplicationSetting> _appSettingRepo;
        private readonly IBankManager _bankManager;

        public AppSettingController(IRepository<string, ApplicationSetting> appSettingRepo, IBankManager bankManager)
        {
            _appSettingRepo = appSettingRepo;
            _bankManager = bankManager;
        }

        public IActionResult Index()
        {
            var settings = _appSettingRepo.AsQueryable();
            return View(settings);
        }

        [HttpPost]
        public IActionResult Index(AppSettingVm model)
        {
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.AccountId).Value = model.AccountId;
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.SecretKey).Value = model.SecretKey;
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.PublicKey).Value = model.PublicKey;
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.TaxPercent).Value = model.TaxPercentage;

            _appSettingRepo.SaveChanges();

            var settings = _appSettingRepo.AsQueryable();
            return View(settings);
        }

        public IActionResult DisconnectAccount()
        {
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.AccountId).Value = "";
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.SecretKey).Value = "";
            _appSettingRepo.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.PublicKey).Value = "";
            _appSettingRepo.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Authenticate(string token)
        {
            await _bankManager.AuthenticateToken(token);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SynchronizeTransactions()
        {
            //await _bankManager.SynchronizeWithMono();
            return RedirectToAction(nameof(Index));
        }
        
    }
}