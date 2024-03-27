namespace Monochrome.Module.Core.Models
{
    public class LoanGuarantor : BaseModel
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public long LoanId { get; set; }
        public Loan Loan { get; set; }
        public decimal AmountToVouch { get; set; }
        public decimal AmountRequested { get; set; }
        public ApplicationStatus Status { get; set; }
        public string Comment { get; set; }
    }
}
