using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class LoanApplyForm
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Disbursement Account")]
        [Remote(action: "NameInquiry", controller: "UserLoans", areaName: "Admin", AdditionalFields = nameof(BankCode))]
        [StringLength(maximumLength:10, MinimumLength = 10)]
        public string DisbursementAccount { get; set; }

        [Required(ErrorMessage ="Please select a bank")]
        [Display(Name = "Disbursement Account")]
        public string BankCode { get; set; }

        [Required]
        public ushort Tenure { get; set; }

        public string Guarantors { get; set; }

        public IFormFileCollection Documents { get; set; }
    }
}