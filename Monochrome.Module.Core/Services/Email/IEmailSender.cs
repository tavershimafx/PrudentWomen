namespace Monochrome.Module.Core.Services.Email
{
    public interface IEmailSender
    {
        DeliveryResponse SendEmail(EmailMessage message, string userDisplayName = null);

        DeliveryResponse SendEmail(EmailMessage message, string[] from, string username = null, string password = null, string userDisplayName = null);

        Task<DeliveryResponse> SendEmailAsync(EmailMessage message, string userDisplayName = null);

        Task<DeliveryResponse> SendEmailAsync(EmailMessage message, string[] from, string username = null, string password = null, string userDisplayName = null);

        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}