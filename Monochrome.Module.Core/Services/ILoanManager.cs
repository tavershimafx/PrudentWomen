using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.Helpers;

namespace Monochrome.Module.Core.Services
{
    public interface ILoanManager
    {
        Result<string> Apply(LoanApplyForm form, string userId);

        Result<bool> Approve(LoanApprovalForm form, string approverUserName);

        Result<bool> Reject(long loanId, string comment, string rejectorUserName);
    }
}
