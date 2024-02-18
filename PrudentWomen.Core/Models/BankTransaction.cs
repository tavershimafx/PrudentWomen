
//using Monochrome.Module.Core.Models;

//namespace PrudentWomen.Core.Models
//{
//    public class TransactionResponse
//    {
//        public string status { get; set; }
//        public string message { get; set; }
//        public Pagination data { get; set; }
//        public List<BankTransaction> transactions { get; set; }
//    }
//    public class Pagination
//    {
//        public int page { get; set; }
//        public int total { get; set; }
//        public string previous { get; set; }
//        public string next { get; set; }
//    }

//    public class BankTransaction
//    {
//        public string id { get; set; }
//        public string type { get; set; }
//        public decimal amount { get; set; }
//        public string narration { get; set; }
//        public string date { get; set; }
//        public decimal balance { get; set; }
//        public string currency { get; set; }
//        public bool IsIdentified { get; set; }
//    }
//    public class UserAccount : BaseModel
//    {
//        public decimal Balance { get; set; }
//        public string UserId { get; set; }
//        public User User { get; set; }
//        public DateTimeOffset LastTransaction { get; set; }
//    }
//    public class UserTransaction: BaseModel
//    {
//        public string Type { get; set; }
//        public decimal Amount { get; set; }
//        public DateTimeOffset Date { get; set; }
//        public decimal Balance { get; set; }
//    }
//    public class Loan : BaseModel
//    {
//        public decimal AmountRequested { get; set; }
//        public decimal AmountGranted { get; set; }
//        public ushort PecentInterest { get; set; }
//        public DateTimeOffset DateApplied { get; set; }
//        public bool Approved { get; set; }
//        public string ApproverId { get; set; }
//        public User Approver { get; set; }
//        public decimal BalanceAtApproval { get; set; }
//        public DateTimeOffset DateApproved { get; set; }
//        public bool Repaid { get; set; }
//        public string Comments { get; set; }
//        public DateTimeOffset DateDisbursed { get; set; }
//    }
//    public class ApplicationSetting : BaseModel
//    {
//        public string Key { get; set; }
//        public string Value { get; set; }
//    }
//}
