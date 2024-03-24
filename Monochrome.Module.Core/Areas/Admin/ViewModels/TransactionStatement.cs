namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class TransactionStatement
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}