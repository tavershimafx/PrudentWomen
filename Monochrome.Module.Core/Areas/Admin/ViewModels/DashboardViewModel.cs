﻿
namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class UserAccountList
    {
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
    }
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; }
        public int TotalLoans { get; set; }
        public decimal Balance { get; set; }
        public decimal UnPaidLoans { get; set; }
        public string HighestBalanceUserName { get; set; }
        public decimal? HighestBalance { get; set; }
        public decimal? LowestBalance { get; set; }
        public string LowestBalanceUserName { get; set; }
        public decimal TotalOverdue { get; set; }
        public DateTimeOffset FromOneYearDate { get; set; }
        public DateTimeOffset MaximumDate { get; set; }
        public IEnumerable<object[]> Debit { get; set; }
        public IEnumerable<object[]> Credit { get; set; }
        public IEnumerable<LoanList> PendingLoans { get; set; }
    }
}