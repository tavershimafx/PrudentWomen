using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.Services
{
    public interface ILoanManager
    {
        Task<Result<string>> Apply(LoanApplyForm form, string userId);

        Result<bool> Approve(LoanApprovalForm form, string approverUserName);

        Result<bool> Reject(long loanId, string comment, string rejectorUserName);

        Task<Result<string>> DisburseLoan(long id);

        IEnumerable<LoanList> GetUnpaidLoans();

        Result<string> AddRepayment(long transactionId, long loanId);
    }
}
