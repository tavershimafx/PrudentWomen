using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Models;
using Newtonsoft.Json;
using PrudentWomen.Core.Extensions;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace PrudentWomen.Core.Services
{
    public class BankManager : IBankManager
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<UserTransaction> _userTransaction;

        public BankManager(HttpClient httpClient, IConfiguration configuration, IRepository<string, User> userRepository,
            IRepository<UserAccount> userAccount, IRepository<UserTransaction> userTransaction)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _userRepository = userRepository;
            _userAccount = userAccount;
            _userTransaction = userTransaction;
        }

        private async Task IdentifyAndMapTransactions(List<BankTransaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                Regex regex = new Regex("PWC-[\\d+]{5,6}", RegexOptions.IgnoreCase);
                var match = regex.Match(transaction.narration);
                if (match.Success)
                {
                    string username = match.Value;
                    var user = _userRepository.AsQueryable().FirstOrDefault(k => k.UserName == username);
                    if (user != null)
                    {
                        _userTransaction.Insert(new UserTransaction()
                        {
                            Amount = transaction.amount,
                            Type = transaction.type,
                            Date = DateTime.Parse(transaction.date),
                            Balance = 0
                        });

                        transaction.IsIdentified = true;
                    }
                }
            }

            await _userTransaction.SaveChangesAsync();
        }

        public async Task FetchTransactions()
        {
            string token = await AuthenticateToken();
            string url = $"{_configuration["MonoApi:BaseUrl"]}/{token}{_configuration["MonoApi:Transactions"]}";
            string filters = $"?paginate=false&start={DateTime.Now}&end={DateTime.Now}";
            url = $"{url}/{filters}";

            _httpClient.DefaultRequestHeaders.Add("mono-sec-key", _configuration["MonoApi:SecretKey"]);
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("x-realtime", _configuration["MonoApi:RealTime"]);
            var response = await _httpClient.GetAsync(url);

            var nn = await response.Content.ReadAsStringAsync();
            var ss = "";
        }

        private async Task<string> AuthenticateToken()
        {
            string url = $"{_configuration["MonoApi:BaseUrl"]}{_configuration["MonoApi:Token"]}";
            _httpClient.DefaultRequestHeaders.Add("mono-sec-key", _configuration["MonoApi:SecretKey"]);
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            var code = new { code = _configuration["MonoApi:PublicKey"] };
            var response = await _httpClient.PostAsync(url, code.SerializeObject());

            var nn = await response.Content.ReadAsStringAsync();
            dynamic ss = JsonConvert.DeserializeObject<object>(nn);
            return ss.id;
        }
    }
}
