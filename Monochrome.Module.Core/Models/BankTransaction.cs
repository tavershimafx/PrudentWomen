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

    public class BankTransaction
    {
        public string id { get; set; }
        public string type { get; set; }
        public decimal amount { get; set; }
        public string narration { get; set; }
        public string date { get; set; }
        public decimal balance { get; set; }
        public string currency { get; set; }
        public bool IsIdentified { get; set; }
    }
}
