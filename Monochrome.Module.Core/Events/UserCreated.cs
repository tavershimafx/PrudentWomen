using MediatR;

namespace Monochrome.Module.Core.Events
{
    public class UserCreated: INotification
    {
        public string UserId { get; set; }
    }
}
