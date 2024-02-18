namespace Monochrome.Module.Core.Services.Email
{
    public class SmtpConfiguration
    {
        public int Port { get; set; }
        public string Server { get; set; }
        public string DisplayName { get; set; }
        public string NoReplyEmail { get; set; }
        public string NoReplyPassword { get; set; }
    }
}