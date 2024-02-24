using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.Services
{
    public interface IBankManager
    {
        Task SynchronizeWithMono(long syncId);

        Task<bool> AuthenticateToken(string token);
    }
}
