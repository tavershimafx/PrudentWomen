namespace Monochrome.Module.Core.Models
{
    public class Loan : BaseModel
    {
        public decimal AmountRequested { get; set; }
        public decimal AmountGranted { get; set; }
        public ushort PecentInterest { get; set; }
        public DateTimeOffset DateApplied { get; set; }
        public bool Approved { get; set; }
        public string ApproverId { get; set; }
        public User Approver { get; set; }
        public decimal BalanceAtApproval { get; set; }
        public DateTimeOffset DateApproved { get; set; }
        public bool Repaid { get; set; }
        public string Comments { get; set; }
        public DateTimeOffset DateDisbursed { get; set; }
    }
}
