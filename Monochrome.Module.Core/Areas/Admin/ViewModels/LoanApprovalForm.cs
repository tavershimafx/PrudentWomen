using Monochrome.Module.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class LoanApprovalForm
    {
        [Required]
        public long Id { get; set; }

        [Required(ErrorMessage = "Amount to grant must be entered")]
        public decimal? AmountGranted { get; set; }

        public bool Disburse { get; set; }

        [Required(ErrorMessage = "You need to enter some comments for approval")]
        public string Comments { get; set; }
    }
}