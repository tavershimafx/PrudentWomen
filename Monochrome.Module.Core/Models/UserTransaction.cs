namespace Monochrome.Module.Core.Models
{
    public class UserTransaction: BaseModel
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Date { get; set; }
        public decimal Balance { get; set; }
    }
}
