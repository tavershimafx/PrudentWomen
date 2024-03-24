namespace Monochrome.Module.Core.Models
{
    public class LoanApprovalDetails: Loan
    {
        public string DisbursementAccountName { get; set; }
        public static LoanApprovalDetails FromLoan(Loan loan)
        {
            return new LoanApprovalDetails()
            {
                Id = loan.Id,
                AmountGranted = loan.AmountGranted,
                AmountRequested = loan.AmountRequested,
                Approver = loan.Approver,
                ApproverId = loan.ApproverId,
                BalanceAtApproval = loan.BalanceAtApproval,
                DateApplied = loan.DateApplied,
                DateApproved = loan.DateApproved,
                DisbursementAccount = loan.DisbursementAccount,
                UserAccount = loan.UserAccount,
                UserAccountId = loan.UserAccountId,
                Comments = loan.Comments,
                ConcurrencyStamp = loan.ConcurrencyStamp,
                CreatedBy = loan.CreatedBy,
                CreatedById = loan.CreatedById,
                DateCreated = loan.DateCreated,
                DateDisbursed = loan.DateDisbursed,
                LastUpdated = loan.LastUpdated,
                PecentInterest = loan.PecentInterest,
                Repaid = loan.Repaid,
                Status = loan.Status,
                Tenure = loan.Tenure,
                UpdatedBy = loan.UpdatedBy,
                UpdatedById = loan.UpdatedById,
                SupportingDocuments = loan.SupportingDocuments,
                Guarantors = loan.Guarantors
            };
        }
        public IEnumerable<Loan> OutstandingLoans { get; set; }
    }
}
