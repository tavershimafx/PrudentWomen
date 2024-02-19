using System.ComponentModel.DataAnnotations;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class LoanApplyForm
    {
        public decimal Amount { get; set; }

        [StringLength(maximumLength:10, MinimumLength = 10)]
        public string DisbursementAccount { get; set; }

        public ushort Tenure { get; set; }

        public string Comment { get; set; }
    }
}