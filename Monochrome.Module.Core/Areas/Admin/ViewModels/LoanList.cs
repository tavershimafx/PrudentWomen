using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class LoanList
    {
        public long Id { get; set; }
        public decimal AmountRequested { get; set; }
        public ushort Tenure { get; set; }
        public DateTimeOffset DateApplied { get; set; }
        public ApplicationStatus Status { get; set; }
        public string UserAccount { get; set; }
       
        public DateTimeOffset? DateApproved { get; set; }
        public bool Repaid { get; set; }
        public bool Disbursed { get; set; }
    }
}