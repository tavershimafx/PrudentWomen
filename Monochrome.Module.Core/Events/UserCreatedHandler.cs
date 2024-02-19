using MediatR;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Events;
using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.Events
{
    public class UserCreatedHandler : INotificationHandler<UserCreated>
    {
        private readonly IRepository<UserAccount> _userAccount;

        public UserCreatedHandler(IRepository<UserAccount> userAccount)
        {
            _userAccount = userAccount;
        }

        public async Task Handle(UserCreated notification, CancellationToken cancellationToken)
        {
            var account = new UserAccount
            {
                UserId = notification.UserId,
            };

            _userAccount.Insert(account);
            await _userAccount.SaveChangesAsync();
        }
    }
}
