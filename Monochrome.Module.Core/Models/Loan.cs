﻿namespace Monochrome.Module.Core.Models
{
    public class Loan : BaseModel
    {
        public decimal AmountRequested { get; set; }
        public decimal AmountGranted { get; set; }
        public ushort PecentInterest { get; set; }
        /// <summary>
        /// The loan tenure in months
        /// </summary>
        public ushort Tenure { get; set; }
        public DateTimeOffset DateApplied { get; set; }
        public bool Approved { get; set; }
        public long UserAccountId { get; set; }
        public UserAccount UserAccount { get; set; }
        public string ApproverId { get; set; }
        public User Approver { get; set; }
        public decimal BalanceAtApproval { get; set; }
        public DateTimeOffset DateApproved { get; set; }
        public bool Repaid { get; set; }
        public string Comments { get; set; }
        public DateTimeOffset DateDisbursed { get; set; }
        public string DisbursementAccount { get; set; }
    }
}
