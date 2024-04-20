using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;
using Monochrome.Module.Core.Services.Email;
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
        private readonly IRepository<string, Role> _roleRepo;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<UserTransaction> _userTransaction;
        private readonly IRepository<BankTransaction> _bankTransactionRepo;
        private readonly IRepository<string, ApplicationSetting> _appSetting;
        private readonly IRepository<SyncLog> _syncLogs;
        private readonly IEmailSender _emailSender;

        public BankManager(IHttpClientFactory httpClient, IConfiguration configuration, IRepository<string, User> userRepository,
            IRepository<UserAccount> userAccount, IRepository<UserTransaction> userTransaction,
            IRepository<string, ApplicationSetting> appSetting, IRepository<BankTransaction> bankTransactionRepo,
            IRepository<SyncLog> syncLogs, IRepository<string, Role> roleRepo, IEmailSender emailSender)
        {
            _httpClientFactory = httpClient;
            _configuration = configuration;
            _userRepository = userRepository;
            _userAccount = userAccount;
            _userTransaction = userTransaction;
            _bankTransactionRepo = bankTransactionRepo;
            _appSetting = appSetting;
            _syncLogs = syncLogs;
            _roleRepo = roleRepo;
            _emailSender = emailSender;
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
                    var opening = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.OpeningDate);
                    DateTime openingDate = DateTime.MinValue;
                    bool converted = false;
                    if (!string.IsNullOrEmpty(opening.Value))
                    {
                        converted = DateTime.TryParse(opening.Value, out openingDate);
                    }

                    if (!string.IsNullOrEmpty(opening.Value) && !converted)
                    {
                        throw new InvalidOperationException("An invalid transaction date has been set on the" +
                        " opening date. Please correct this and try again");
                    }

                    if (!string.IsNullOrEmpty(opening.Value))
                    {
                        if (existing.Date >= openingDate)
                        {
                            var matched = MatchTransaction(transaction);
                            if (matched != null)
                            {
                                usrTrans.Add(matched);
                                transaction.IsIdentified = true;
                                transaction.ManualMap = false;
                            }
                        }
                    }
                    else
                    {
                        var matched = MatchTransaction(transaction);
                        if (matched != null)
                        {
                            usrTrans.Add(matched);
                            transaction.IsIdentified = true;
                            transaction.ManualMap = false;
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
                var opening = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.OpeningDate);
                DateTime openingDate = DateTime.MinValue;
                bool converted = false;
                if (!string.IsNullOrEmpty(opening.Value))
                {
                    converted = DateTime.TryParse(opening.Value, out openingDate);
                }

                if (!string.IsNullOrEmpty(opening.Value) && !converted)
                {
                    return new Result<bool>() { Succeeded = false, Error = "An invalid transaction date has been set " +
                        "on the opening date. Please correct this and try again" };
                }

                if ((!string.IsNullOrEmpty(opening.Value) && existing.Date >= openingDate) || string.IsNullOrEmpty(opening.Value))
                {
                    if (!existing.IsIdentified)
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
                                Balance = 0,
                                Narration = existing.Narration
                            });

                            if (existing.Type == "credit")
                            {
                                account.Balance += existing.Amount;
                            }
                            else if (existing.Type == "debit")
                            {
                                account.Balance -= existing.Amount;
                            }
                            else
                            {
                                throw new InvalidOperationException("Cannot identify the type of transaction to be performed");
                            }

                            existing.IsIdentified = true;
                            existing.ManualMap = true;

                            await _bankTransactionRepo.SaveChangesAsync();
                            return new Result<bool>() { Succeeded = true };
                        }

                        return new Result<bool>() { Succeeded = false, Error = "Unable to find user!" };
                    }

                    return new Result<bool>() { Succeeded = false, Error = "This transaction has already been mapped!" };
                }

                return new Result<bool>() { Succeeded = false, Error = "The transaction you attempted mapping " +
                    "was performed before the opening date set by the administrator. Please verify this and try again." };
               
            }

            return new Result<bool>() { Succeeded = false, Error = "Transaction does not exist." };
        }

        public async Task<Result<bool>> BulkIdentify(string transactionId, IEnumerable<BulkEntryItem> bulkUsers)
        {
            var existing = _bankTransactionRepo.AsQueryable().FirstOrDefault(k => k._Id == transactionId);
            if (existing != null && bulkUsers.Any())
            {
                var opening = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.OpeningDate);
                DateTime openingDate = DateTime.MinValue;
                bool converted = false;
                if (!string.IsNullOrEmpty(opening.Value))
                {
                    converted = DateTime.TryParse(opening.Value, out openingDate);
                }

                if (!string.IsNullOrEmpty(opening.Value) && !converted)
                {
                    return new Result<bool>()
                    {
                        Succeeded = false,
                        Error = "An invalid transaction date has been set " +
                        "on the opening date. Please correct this and try again"
                    };
                }

                if ((!string.IsNullOrEmpty(opening.Value) && existing.Date >= openingDate) || string.IsNullOrEmpty(opening.Value))
                {
                    if (!existing.IsIdentified)
                    {
                        if (bulkUsers.Sum(k => k.Amount) * 100 != existing.Amount)
                        {
                            return new Result<bool>()
                            {
                                Succeeded = false,
                                Error = "Cannot perform the operation because the total " +
                                "amount inputed is not equal to the amount recorded by the transaction."
                            };
                        }

                        var validateResponse = ValidateUsers(bulkUsers.Select(k => k.Username));
                        if (validateResponse.Succeeded)
                        {
                            List<UserAccount> transactingAccounts = new();
                            foreach (var bulkItem in bulkUsers)
                            {
                                var user = _userRepository.AsQueryable().FirstOrDefault(k => k.UserName == bulkItem.Username);
                                var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
                                _userTransaction.Insert(new UserTransaction()
                                {
                                    UserAccountId = account.Id,
                                    Amount = bulkItem.Amount * 100,
                                    Type = existing.Type,
                                    Date = existing.Date,
                                    Balance = 0,
                                    Narration = existing.Narration
                                });

                                if (existing.Type == "credit")
                                {
                                    account.Balance += bulkItem.Amount * 100;
                                }
                                else if (existing.Type == "debit")
                                {
                                    account.Balance -= bulkItem.Amount * 100;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Cannot identify the type of transaction to be performed");
                                }

                                transactingAccounts.Add(account);
                            }

                            existing.IsIdentified = true;
                            existing.ManualMap = true;

                            await _bankTransactionRepo.SaveChangesAsync();
                            return new Result<bool>() { Succeeded = true };
                        }

                        return new Result<bool>() { Succeeded = false, Error = validateResponse.Error };
                    }

                    return new Result<bool>() { Succeeded = false, Error = "This transaction has already been mapped!" };
                }

                return new Result<bool>()
                {
                    Succeeded = false,
                    Error = "The transaction you attempted mapping " +
                    "was performed before the opening date set by the administrator. Please verify this and try again."
                };
            }

            return new Result<bool>() { Succeeded = false, Error = "Transaction does not exist or no users selected" };
        }

        public BankTransaction GetTransaction(long transactionId)
        {
            return _bankTransactionRepo.AsQueryable().FirstOrDefault(k => k.Id == transactionId);
        }

        public UserAccount GetAccount(string userId)
        {
            return _userAccount.AsQueryable().FirstOrDefault(k => k.UserId == userId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount">Amount in naira</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SetOpeningBalance(long accountId, decimal amount)
        {
            UserAccount account = _userAccount.AsQueryable().FirstOrDefault(b => b.Id == accountId) ?? throw new InvalidOperationException($"Invalid account id: {accountId}");
            var opening = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.OpeningDate);
            DateTime openingDate = DateTime.MinValue;
            bool converted = false;
            if (!string.IsNullOrEmpty(opening.Value))
            {
                converted = DateTime.TryParse(opening.Value, out openingDate);
            }

            if (!converted || openingDate == DateTime.MinValue)
            {
                throw new InvalidOperationException("No date or an invalid transaction date has been set on the" +
                " opening date. Please correct this and try again");
            }

            await _userTransaction.AsQueryable().Where(p => p.Date < openingDate).ForEachAsync(x => x.Type = "invalid");
            _userTransaction.SaveChanges();

            var validTransactions = _userTransaction.AsQueryable().Where(g => g.Date >= openingDate).ToList(); ;
            var debits = validTransactions.Where(n => n.Type == "debit").Sum(n => n.Amount);
            var credits = validTransactions.Where(n => n.Type == "credit").Sum(n => n.Amount);
            account.Balance = (amount * 100) + (credits - debits); // amount converted to kobo before operation

            _userTransaction.Insert(new UserTransaction()
            {
                UserAccountId = account.Id,
                Amount = amount * 100,
                Type = "credit",
                Date = openingDate.Subtract(TimeSpan.FromDays(1)),
                Balance = 0,
                Narration = "Account Opening Balance"
            });
            _userAccount.SaveChanges();
        }

        /// <summary>
        /// Credits the super admin account which represents the account of the organisation the requested 
        /// amount
        /// </summary>
        /// <param name="amount">Amount in kobo</param>
        public void CreditSuperAccount(long sourceAccountId, decimal amount, string creditNarration, string debitNarration = "")
        {
            var superRole = _roleRepo.AsQueryable().FirstOrDefault(k => k.Name == "SuperAdmin");
            var user = _userRepository.AsQueryable()
                .Include(n => n.UserRoles)
                .FirstOrDefault(k => k.UserRoles.Any(p => p.RoleId == superRole.Id));
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);

            var sourceAccount = _userAccount.AsQueryable().Include(p => p.User).FirstOrDefault(n => n.Id == sourceAccountId);
            var sourceTransaction = new UserTransaction()
            {
                Amount = amount,
                UserAccountId = sourceAccount.Id,
                Type = "debit",
                Date = DateTime.Now,
                Narration = string.IsNullOrEmpty(debitNarration)? creditNarration : debitNarration
            };
            sourceAccount.Balance -= amount;

            var userTransaction = new UserTransaction()
            {
                Amount = amount,
                UserAccountId = account.Id,
                Type = "credit",
                Date = DateTime.Now,
                Narration = creditNarration
            };
            account.Balance += amount;

            _userTransaction.Insert(sourceTransaction);
            _userTransaction.Insert(userTransaction);
            _userTransaction.SaveChanges();

            // send email to user
            _emailSender.SendEmail(sourceAccount.User.Email, "Account Debit", $"An transaction just occured on your Prudent Women " +
                $"Coperative account. \nAmount: ₦{(amount / 100):N2}\nNarration: {sourceTransaction.Narration}");

            _emailSender.SendEmail(user.Email, "Account Credit", $"An transaction just occured on your Prudent Women " +
                $"Coperative account. \nAmount: ₦{(amount / 100):N2}\nNarration: {userTransaction.Narration}");
        }

        /// <summary>
        /// Debits the super admin account which represents the account of the organisation the requested 
        /// amount
        /// </summary>
        /// <param name="amount">Amount in kobo</param>
        public void DebitSuperAccount(long destinationAccountId, decimal amount, string creditNarration, string debitNarration = "")
        {
            var superRole = _roleRepo.AsQueryable().FirstOrDefault(k => k.Name == "SuperAdmin");
            var user = _userRepository.AsQueryable()
                .Include(n => n.UserRoles)
                .FirstOrDefault(k => k.UserRoles.Any(p => p.RoleId == superRole.Id));
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);

            var userTransaction = new UserTransaction()
            {
                Amount = amount,
                UserAccountId = account.Id,
                Type = "debit",
                Date = DateTime.Now,
                Narration = string.IsNullOrEmpty(debitNarration) ? creditNarration : debitNarration
            };
            account.Balance -= amount;

            var destinationAccount = _userAccount.AsQueryable().Include(p => p.User).FirstOrDefault(n => n.Id == destinationAccountId);
            var destinationTransaction = new UserTransaction()
            {
                Amount = amount,
                UserAccountId = destinationAccount.Id,
                Type = "credit",
                Date = DateTime.Now,
                Narration = creditNarration
            };
            destinationAccount.Balance += amount;

            _userTransaction.Insert(userTransaction);
            _userTransaction.Insert(destinationTransaction);
            _userTransaction.SaveChanges();

            // send email to user
            // send email to user
            _emailSender.SendEmail(user.Email, "Account Debit", $"An transaction just occured on your Prudent Women " +
                $"Coperative account. \nAmount: ₦{(amount / 100):N2}\nNarration: {userTransaction.Narration}");

            _emailSender.SendEmail(destinationAccount.User.Email, "Account Credit", $"An transaction just occured on your Prudent Women " +
                $"Coperative account. \nAmount: ₦{(amount / 100):N2}\nNarration: {destinationTransaction.Narration}");
        }

        /// <summary>
        /// Credits the designated account of the user with the requested amount
        /// </summary>
        /// <param name="amount">Amount in kobo</param>
        public void ExecuteTransaction(long sourceAccountId, long destinationId, decimal amount, string creditNarration, string debitNarration = "")
        {
            var sourceAccount = _userAccount.AsQueryable().Include(a => a.User).FirstOrDefault(n => n.Id == sourceAccountId);
            var destinationAccount = _userAccount.AsQueryable().Include(a => a.User).FirstOrDefault(n => n.Id == destinationId);
            
            var sourceTransaction = new UserTransaction()
            {
                Amount = amount,
                UserAccountId = sourceAccount.Id,
                Type = "debit",
                Date = DateTime.Now,
                Narration = string.IsNullOrEmpty(debitNarration) ? creditNarration : debitNarration
            };
            sourceAccount.Balance -= amount;

            var destinationTransaction = new UserTransaction()
            {
                Amount = amount,
                UserAccountId = destinationAccount.Id,
                Type = "credit",
                Date = DateTime.Now,
                Narration = creditNarration
            };
            destinationAccount.Balance += amount;

            _userTransaction.Insert(sourceTransaction);
            _userTransaction.Insert(destinationTransaction);
            _userTransaction.SaveChanges();

            // send email to user
            _emailSender.SendEmail(sourceAccount.User.Email, "Account Debit", $"An transaction just occured on your Prudent Women " +
                $"Coperative account. \nAmount: ₦{(amount / 100):N2}\nNarration: {sourceTransaction.Narration}");

            _emailSender.SendEmail(destinationAccount.User.Email, "Account Credit", $"An transaction just occured on your Prudent Women " +
                $"Coperative account. \nAmount: ₦{(amount / 100):N2}\nNarration: {destinationTransaction.Narration}");
        }

        private UserTransaction MatchTransaction(BankTransaction transaction)
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
                    var usrTrans = new UserTransaction()
                    {
                        UserAccountId = account.Id,
                        Amount = transaction.Amount,
                        Type = transaction.Type,
                        Date = transaction.Date,
                        Balance = 0,
                        Narration = transaction.Narration
                    };

                    account.Balance += transaction.Amount;
                    return usrTrans;
                }
            }

            return null;
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

        private Result<bool> ValidateUsers(IEnumerable<string> usernames)
        {
            string errors = "";
            bool hasError = false;
            foreach (var username in usernames)
            {
                var user = _userRepository.AsQueryable().FirstOrDefault(k => k.UserName == username);
                if (user == null)
                {
                    errors += $"The user {username} is invalid or does not exist.\n";
                    hasError = true;
                }
            }

            if (hasError)
            {
                return new Result<bool>() { Succeeded = false, Error = errors };
            }

            return new Result<bool>() { Succeeded = true };
        }
    }
}
