using Hangfire;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.Services
{
    public interface IBankManager
    {
        [AutomaticRetry(Attempts = 0)]
        Task SynchronizeWithMono(DateTime start, DateTime end, bool fromPrev = true);

        Task<Result<bool>> ManualIdentify(string transactionId, string userName);

        Task<Result<bool>> BulkIdentify(string transactionId, IEnumerable<BulkEntryItem> bulkUsers);

        Task<bool> AuthenticateToken(string token);

        Task<Result<IEnumerable<Bank>>> FetchBanks();

        Task<Result<AccountLookup>> AccountLookup(AccountLookupObject model);

        UserAccount GetAccount(string userId);

        BankTransaction GetTransaction(long transactionId);

        void CreditSuperAccount(long sourceAccountId, decimal amount, string creditNarration, string debitNarration = "");

        void DebitSuperAccount(long destinationAccountId, decimal amount, string creditNarration, string debitNarration = "");

        void ExecuteTransaction(long sourceAccountId, long destinationId, decimal amount, string creditNarration, string debitNarration = "");
    }
}
