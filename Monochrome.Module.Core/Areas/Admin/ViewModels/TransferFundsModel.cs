namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class TransferFundsModel
    {
        public long SourceAccountId { get; set; }
        public string DestinationUser { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
    }
}