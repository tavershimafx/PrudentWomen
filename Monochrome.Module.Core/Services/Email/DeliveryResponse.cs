namespace Monochrome.Module.Core.Services.Email
{
    /// <summary>
    /// Represents an email response
    /// </summary>
    public struct DeliveryResponse
    {
        public bool Succedded { get; set; }
        public IList<string> Errors { get; set; }
    }
}