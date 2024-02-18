using Microsoft.AspNetCore.Http;
using MimeKit;

namespace Monochrome.Module.Core.Services.Email
{
    public class EmailMessage
    {
        private List<MailboxAddress> _to { get; set; }

        /// <summary>
        /// Constructs a new email message with provision for attaching media files to it.
        /// </summary>
        /// <param name="to">The recipient of the message.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="content">the contents of the message.</param>
        /// is a mixed message.</param>
        public EmailMessage(string[] to, string subject, string content, IFormFileCollection attachments)
        {
            _to = new List<MailboxAddress>();
            _to.AddRange(to.Select(x => MailboxAddress.Parse(x)));
            Subject = subject;
            Content = content;
            Attachments = attachments;
        }

        /// <summary>
        /// Constructs a new email message.
        /// </summary>
        /// <param name="to">The recipient of the message.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="content">the contents of the message.</param>
        /// <param name="username"></param>
        /// is a mixed message.</param>
        public EmailMessage(string[] to, string subject, string content)
        {
            _to = new List<MailboxAddress>();
            _to.AddRange(to.Select(x => MailboxAddress.Parse(x)));
            Subject = subject;
            Content = content;
        }

        /// <summary>
        /// The recipients of the message
        /// </summary>
        public List<MailboxAddress> To
        {
            get { return _to; }
        }

        /// <summary>
        /// The subject of the message
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// The Main content of the message. Can be Html or string.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The media attachments to the message
        /// </summary>
        public IFormFileCollection Attachments { get; }
    }
}