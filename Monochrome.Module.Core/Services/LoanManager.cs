using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.Services
{
    public class LoanManager: ILoanManager
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<string, User> _userRepo;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<string, ApplicationSetting> _appSetting;

        public LoanManager(IRepository<Loan> loanRepo, IRepository<string, User> userRepo,
            IRepository<UserAccount> userAccount, IRepository<string, ApplicationSetting> appSetting)
        {
            _loanRepo = loanRepo;
            _userRepo = userRepo;
            _userAccount = userAccount;
            _appSetting = appSetting;
        }

        public Result<string> Apply(LoanApplyForm form, string userName)
        {
            var user = _userRepo.AsQueryable().FirstOrDefault(k => k.UserName == userName);
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
            var interest = _appSetting.AsQueryable().FirstOrDefault(b => b.Id == ApplicationConstants.PercentInterest);
            if (!string.IsNullOrEmpty(interest.Value) && ushort.Parse(interest.Value) > 0)
            {
                var loan = new Loan
                {
                    AmountRequested = form.Amount,
                    DisbursementAccount = form.DisbursementAccount,
                    Tenure = form.Tenure,
                    UserAccountId = account.Id,
                    PecentInterest = ushort.Parse(interest.Value),
                    Status = LoanApplyStatus.Pending,
                    DateApplied = DateTime.Now,
                    BankNIPCode = form.BankCode
                };

                _loanRepo.Insert(loan);
                _loanRepo.SaveChanges();
                return new Result<string> { Succeeded = true, Error = "Loan Application successful." };
            }

            return new Result<string> { Succeeded = false, Error = "PI: Unable to proceed with loan application. Please contact the administrator." };
        }

        public Result<bool> Reject(long loanId, string comment, string rejectorUserName)
        {
            var approver = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == rejectorUserName);
            return new Result<bool>() { Succeeded = true };
        }

        public Result<bool> Approve(LoanApprovalForm form, string approverUserName)
        {
            Loan loan = _loanRepo.AsQueryable().FirstOrDefault(n => n.Id == form.Id);
            if (loan == null)
                throw new InvalidOperationException($"Cannot approve an inexistent loan with id: {form.Id}");

            var approver = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == approverUserName);
            loan.Status = LoanApplyStatus.Approved;
            loan.AmountGranted = form.AmountGranted.Value;
            loan.ApproverId = approver.Id;
            loan.Comments = form.Comments;
            loan.DateApproved = DateTime.Now;

            if (form.Disburse && DisburseLoan(loan.AmountGranted, loan.DisbursementAccount))
            {
                loan.DateDisbursed = DateTime.Now;
            }

            _loanRepo.SaveChanges();
            return new Result<bool>() { Succeeded = true };
        }

        private bool DisburseLoan(decimal amount, string account)
        {
            return true;
        }
    }
}
