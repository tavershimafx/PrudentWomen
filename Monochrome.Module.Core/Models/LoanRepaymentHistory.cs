namespace Monochrome.Module.Core.Models
{
    public class LoanRepaymentHistory : BaseModel
    {
        public long LoanId { get; set; }
        public Loan Loan { get; set; }
        public decimal Amount { get; set; }
    }
}
