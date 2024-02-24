namespace Monochrome.Module.Core.Models
{
    public class TransactionResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public Pagination data { get; set; }
        public List<BankTransaction> transactions { get; set; }
    }
    public class Pagination
    {
        public int page { get; set; }
        public int total { get; set; }
        public string previous { get; set; }
        public string next { get; set; }
    }

    public class BankTransaction : BaseModel<string>
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        public string Date { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public bool IsIdentified { get; set; }
    }
}
