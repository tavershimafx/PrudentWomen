namespace Monochrome.Module.Core.Models
{
    public class LoanDisbursement : BaseModel<string>
    {
        public long LoanId { get; set; }
        public Loan Loan { get; set; }
        public decimal Amount { get; set; }
        public string PaymentGatewayReference { get; set; }
        public string GatewayErrorMessage { get; set; }
        public DisbursementStatus Status { get; set; }
        public string DisbursementAccount { get; set; }
        public DateTimeOffset DateDisbursed { get; set; }
    }
    public class LoanDisbursementList : LoanDisbursement
    {
        public string UserAccount { get; set; }
        public string UserName { get; set; }
    }
    public enum DisbursementStatus
    {
        Successful,

        Pending,

        Failed
    }
}
