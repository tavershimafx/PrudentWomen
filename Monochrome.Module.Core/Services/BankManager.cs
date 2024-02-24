using Microsoft.Extensions.Configuration;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Monochrome.Module.Core.Services
{
    public class BankManager : IBankManager
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<UserTransaction> _userTransaction;
        private readonly IRepository<string, BankTransaction> _bankTransactionRepo;
        private readonly IRepository<string, ApplicationSetting> _appSetting;
        private readonly IRepository<SyncLogs> _syncLogs;

        public BankManager(HttpClient httpClient, IConfiguration configuration, IRepository<string, User> userRepository,
            IRepository<UserAccount> userAccount, IRepository<UserTransaction> userTransaction,
            IRepository<string, ApplicationSetting> appSetting, IRepository<string, BankTransaction> bankTransactionRepo,
            IRepository<SyncLogs> syncLogs)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _userRepository = userRepository;
            _userAccount = userAccount;
            _userTransaction = userTransaction;
            _bankTransactionRepo = bankTransactionRepo;
            _appSetting = appSetting;
            _syncLogs = syncLogs;
        }

        /// <summary>
        /// Performs a fetch operation from mono of the transactions fetched from the connected bank
        /// </summary>
        public async Task SynchronizeWithMono(long syncId)
        {
            _syncLogs.AsQueryable().FirstOrDefault(a => a.Id == syncId).Status = SynchronizationStatus.Running;
            _syncLogs.SaveChanges();

            var start = DateTime.Now.Subtract(TimeSpan.FromDays(30));
            var end = DateTime.Now;
            var transactions = await FetchTransactions(start, end);
            await IdentifyAndMapTransactions(syncId, transactions, start, end);
        }

        /// <summary>
        /// Calls the mono sync endpoint to synchronize data from the connected bank into mono
        /// </summary>
        public void SynchronizeWithBank()
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            string url = $"{_configuration["MonoApi:BaseUrl"]}/v1/accounts/{accountSetting.Id}{_configuration["MonoApi:Transactions"]}";
            string filters = $"?paginate=false&start={DateTime.Now.Subtract(TimeSpan.FromDays(120)):dd-MM-yyyy}&end={DateTime.Now:dd-MM-yyyy}";
            url = $"{url}/{filters}";

            _httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);
            //_httpClient.DefaultRequestHeaders.Add("x-realtime", _configuration["MonoApi:RealTime"]);
            //var response = await _httpClient.GetAsync(url);
        }

        public async Task<bool> AuthenticateToken(string token)
        {
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            string url = $"{_configuration["MonoApi:BaseUrl"]}{_configuration["MonoApi:Athorization"]}";
            _httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);

            var code = new { code = token };
            var response = await _httpClient.PostAsync(url, code.SerializeObject());

            var nn = await response.Content.ReadAsStringAsync();
            dynamic ss = JsonConvert.DeserializeObject<object>(nn);

            _appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.AccountId).Value = ss.id;
            _appSetting.SaveChanges();

            return true;
        }
        
        private async Task IdentifyAndMapTransactions(long syncId, IEnumerable<BankTransaction> transactions, DateTime start, DateTime end)
        {
            foreach (var transaction in transactions)
            {
                // In case our fetch span into an already fetched date range
                var existing = _bankTransactionRepo.AsQueryable().FirstOrDefault(k => k.Id == transaction.Id);
                if (existing == null)
                {
                    Regex regex = new Regex("PWC-[\\d+]{5,6}", RegexOptions.IgnoreCase);
                    var match = regex.Match(transaction.Narration);
                    if (match.Success)
                    {
                        string username = match.Value;
                        var user = _userRepository.AsQueryable().FirstOrDefault(k => k.UserName == username);
                        if (user != null)
                        {
                            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
                            _userTransaction.Insert(new UserTransaction()
                            {
                                AccountId = account.Id,
                                Amount = transaction.Amount,
                                Type = transaction.Type,
                                Date = DateTime.Parse(transaction.Date),
                                Balance = 0
                            });

                            account.Balance += transaction.Amount;
                            transaction.IsIdentified = true;
                        }
                    }
                }
            }

            // save sync logs
            var sync = _syncLogs.AsQueryable().FirstOrDefault(n => n.Id == syncId);
            sync.NumberOfRecords = transactions.Count();
            sync.Status = SynchronizationStatus.Completed;
            _bankTransactionRepo.InsertRange(transactions);
            await _userTransaction.SaveChangesAsync();
        }

        private async Task<IEnumerable<BankTransaction>> FetchTransactions(DateTime start, DateTime end)
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");
            
            string url = $"{_configuration["MonoApi:BaseUrl"]}/v1/accounts/{accountSetting.Id}{_configuration["MonoApi:Transactions"]}";
            string filters = $"?paginate=false&start={start:dd-MM-yyyy}&end={end:dd-MM-yyyy}";
            url = $"{url}/{filters}";

            _httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);
            //_httpClient.DefaultRequestHeaders.Add("x-realtime", _configuration["MonoApi:RealTime"]);
            var response = await _httpClient.GetAsync(url);

            var nn = JsonConvert.DeserializeObject<IEnumerable<BankTransaction>>(await response.Content.ReadAsStringAsync());
            return nn;
        }

    }
}
