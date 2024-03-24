using Microsoft.Extensions.Configuration;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace Monochrome.Module.Core.Services
{
    public class BankManager : IBankManager
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<UserTransaction> _userTransaction;
        private readonly IRepository<BankTransaction> _bankTransactionRepo;
        private readonly IRepository<string, ApplicationSetting> _appSetting;
        private readonly IRepository<SyncLog> _syncLogs;

        public BankManager(IHttpClientFactory httpClient, IConfiguration configuration, IRepository<string, User> userRepository,
            IRepository<UserAccount> userAccount, IRepository<UserTransaction> userTransaction,
            IRepository<string, ApplicationSetting> appSetting, IRepository<BankTransaction> bankTransactionRepo,
            IRepository<SyncLog> syncLogs)
        {
            _httpClientFactory = httpClient;
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
        public async Task SynchronizeWithMono(DateTime start, DateTime end, bool fromPrev = true)
        {
            await SynchronizeWithBank();
            start = start == default ? DateTime.Now.Subtract(TimeSpan.FromDays(30)) : start;
            end = end == default ? DateTime.Now : end;

            var log = new SyncLog { StartDate = start, EndDate = end, Status = SynchronizationStatus.Running };
            if (fromPrev)
            {
                var lastSync = _syncLogs.AsQueryable()
                    .Where(b => b.Status == SynchronizationStatus.Completed)
                    .OrderByDescending(n => n.EndDate)
                    .FirstOrDefault();

                if (lastSync != null)
                {
                    log.StartDate = lastSync.EndDate;
                    log.EndDate = DateTime.Now;
                }
            }

            _syncLogs.Insert(log);
            _syncLogs.SaveChanges();

            var result = await FetchTransactions(log.StartDate.DateTime, log.EndDate.DateTime);
            if (result.Succeeded)
            {
                await IdentifyAndMapTransactions(log.Id, result.Data);
            }
            else
            {
                log.Status = SynchronizationStatus.Failed;
                log.Message = result.Error;
                _syncLogs.SaveChanges();
            }
        }

        /// <summary>
        /// Calls the mono sync endpoint to synchronize data from the connected bank into mono
        /// </summary>
        private async Task SynchronizeWithBank()
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            string url = $"{_configuration["MonoApi:BaseUrl"]}/v1/accounts/{accountSetting.Value}{_configuration["MonoApi:ManualSync"]}";
            string filters = $"?allow_incomplete_statement=false";
            url = $"{url}/{filters}";

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);
            //_httpClient.DefaultRequestHeaders.Add("x-realtime", _configuration["MonoApi:RealTime"]);
            var response = await httpClient.GetAsync(url);
            var kk = await response.Content.ReadAsStringAsync();
            await Task.Delay(2000);
        }

        public async Task<bool> AuthenticateToken(string token)
        {
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            HttpClient httpClient = _httpClientFactory.CreateClient();
            string url = $"{_configuration["MonoApi:BaseUrl"]}{_configuration["MonoApi:Athorization"]}";
            httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);

            var code = new { code = token };
            var response = await httpClient.PostAsync(url, code.SerializeObject());

            var nn = await response.Content.ReadAsStringAsync();
            dynamic ss = JsonConvert.DeserializeObject<object>(nn);

            _appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.AccountId).Value = ss.id;
            _appSetting.SaveChanges();

            return true;
        }

        public async Task<Result<IEnumerable<Bank>>> FetchBanks()
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            string url = $"{_configuration["MonoApi:BaseUrl"]}{_configuration["MonoApi:FetchBanks"]}";
            
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);

            var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (response.IsSuccessStatusCode)
            {
                var tranResponse = JsonConvert.DeserializeObject<ApiResponse<BankData>>(data, jsonSettings);
                return new Result<IEnumerable<Bank>>()
                {
                    Succeeded = true,
                    Data = tranResponse.Data.Banks
                };
            }

            var nn = JsonConvert.DeserializeObject<ApiResponse<BankData>>(data, jsonSettings);
            return new Result<IEnumerable<Bank>>()
            {
                Succeeded = false,
                Error = nn.Message
            };
        }

        public async Task<Result<AccountLookup>> AccountLookup(AccountLookupObject model)
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            string url = $"{_configuration["MonoApi:BaseUrl"]}{_configuration["MonoApi:AccountLookup"]}";

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);

            var response = await httpClient.PostAsync(url, model.SerializeObject());
            var data = await response.Content.ReadAsStringAsync();
            var jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (response.IsSuccessStatusCode)
            {
                var tranResponse = JsonConvert.DeserializeObject<ApiResponse<AccountLookup>>(data, jsonSettings);
                return new Result<AccountLookup>()
                {
                    Succeeded = true,
                    Data = tranResponse.Data
                };
            }

            var nn = JsonConvert.DeserializeObject<ApiResponse<AccountLookup>>(data, jsonSettings);
            return new Result<AccountLookup>()
            {
                Succeeded = false,
                Error = nn.Message
            };
        }

        private async Task IdentifyAndMapTransactions(long syncId, IEnumerable<BankTransaction> transactions)
        {
            List<UserTransaction> usrTrans = new();
            List<BankTransaction> bankTrans = new();
            foreach (var transaction in transactions)
            {
                // In case our fetch span into an already fetched date range
                var existing = _bankTransactionRepo.AsQueryable().FirstOrDefault(k => k._Id == transaction._Id);
                if (existing == null)
                {
                    Regex regex = new Regex("PWC[\\d+]{5,6}", RegexOptions.IgnoreCase);
                    var match = regex.Match(transaction.Narration);
                    if (match.Success)
                    {
                        string username = match.Value;
                        var user = _userRepository.AsQueryable().FirstOrDefault(k => k.UserName == username);
                        if (user != null)
                        {
                            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
                            usrTrans.Add(new UserTransaction()
                            {
                                UserAccountId = account.Id,
                                Amount = transaction.Amount,
                                Type = transaction.Type,
                                Date = transaction.Date,
                                Balance = 0
                            });

                            account.Balance += transaction.Amount;
                            transaction.IsIdentified = true;
                        }
                    }

                    bankTrans.Add(transaction);
                }
            }

            _bankTransactionRepo.InsertRange(bankTrans);
            _userTransaction.InsertRange(usrTrans);

            // save sync logs
            var sync = _syncLogs.AsQueryable().FirstOrDefault(n => n.Id == syncId);
            sync.NumberOfRecords = bankTrans.Count;
            sync.Status = SynchronizationStatus.Completed;
            sync.Message = "Sync Successful";

            await _userTransaction.SaveChangesAsync();
        }

        public async Task<Result<bool>> ManualIdentify(string transactionId, string userName)
        {
            var existing = _bankTransactionRepo.AsQueryable().FirstOrDefault(k => k._Id == transactionId);
            if (existing != null)
            {
                var user = _userRepository.AsQueryable().FirstOrDefault(k => k.UserName == userName);
                if (user != null)
                {
                    var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
                    _userTransaction.Insert(new UserTransaction()
                    {
                        UserAccountId = account.Id,
                        Amount = existing.Amount,
                        Type = existing.Type,
                        Date = existing.Date,
                        Balance = 0
                    });

                    account.Balance += existing.Amount;
                    existing.IsIdentified = true;

                    await _bankTransactionRepo.SaveChangesAsync();
                    return new Result<bool>() { Succeeded = true };
                }

                return new Result<bool>() { Succeeded = false, Error = "Unable to find user!" };
            }

            return new Result<bool>() { Succeeded = false, Error = "Transaction Id does not exist!" };
        }

        private async Task<Result<IEnumerable<BankTransaction>>> FetchTransactions(DateTime start, DateTime end)
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");
            
            string url = $"{_configuration["MonoApi:BaseUrl"]}/v1/accounts/{accountSetting.Value}{_configuration["MonoApi:Transactions"]}";
            string filters = $"?paginate=false&start={start:dd-MM-yyyy}&end={end:dd-MM-yyyy}";
            url = $"{url}/{filters}";

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);
            var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (response.IsSuccessStatusCode)
            {
                var tranResponse = JsonConvert.DeserializeObject<ApiResponse<ResponseData>>(data, jsonSettings);
                return new Result<IEnumerable<BankTransaction>>()
                {
                    Succeeded = true,
                    Data = tranResponse.Data.Transactions
                };
            }

            var nn = JsonConvert.DeserializeObject<ApiResponse<ResponseData>>(data, jsonSettings);
            return new Result<IEnumerable<BankTransaction>>()
            {
                Succeeded = false,
                Error = nn.Message
            };
        }

    }
}
