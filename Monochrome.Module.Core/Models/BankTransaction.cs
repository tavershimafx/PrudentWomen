namespace Monochrome.Module.Core.Models
{
    public class AccountLookupObject
    {
        public string nip_code { get; set; }
        public string account_number { get; set; }
    }

    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string Timestamp { get; set; }
    }

    public class AccountLookup
    {
        public string Name { get; set; }
        public string Account_number { get; set; }
        public string Bvn { get; set; }
        public LookupBank Bank { get; set; }
           
        
    }
    public class LookupBank
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
    public class Bank
    {
        public string Name { get; set; }
        public string Bank_code { get; set; }
        public string Nip_code { get; set; }
    }
    public class BankData
    {
        public IEnumerable<Bank> Banks { get; set; }
    }
   
    public class ResponseData
    {
        public Pagination Paging { get; set; }
        public List<BankTransaction> Transactions { get; set; }
    }
    public class Pagination
    {
        public int Page { get; set; }
        public int Total { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
    }

    public class BankTransaction : BaseModel
    {
        public string _Id { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        public DateTimeOffset Date { get; set; }
        public decimal? Balance { get; set; }
        public string Currency { get; set; }
        public bool IsIdentified { get; set; }
    }
}
