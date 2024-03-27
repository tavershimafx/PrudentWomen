namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class TransactionStatement
    {
        public DateTimeOffset Date { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
    }
}