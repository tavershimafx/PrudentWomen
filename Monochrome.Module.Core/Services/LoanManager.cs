using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Monochrome.Module.Core.Extensions;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Hosting;

namespace Monochrome.Module.Core.Services
{
    public class LoanManager: ILoanManager
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<BankTransaction> _bankTransactionRepo;
        private readonly IRepository<UserTransaction> _userTransactionRepo;
        private readonly IRepository<string, User> _userRepo;
        private readonly IRepository<string, LoanDisbursement> _disbursementRepo;
        private readonly IRepository<UserAccount> _userAccount;
        private readonly IRepository<string, ApplicationSetting> _appSetting;
        private readonly IBankManager _bankManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public LoanManager(IRepository<Loan> loanRepo, IRepository<string, User> userRepo,
            IRepository<UserAccount> userAccount, IRepository<string, ApplicationSetting> appSetting,
            IHttpClientFactory httpClient, IConfiguration configuration, IRepository<string, LoanDisbursement> disbursementRepo,
            IHttpContextAccessor contextAccessor, IWebHostEnvironment environment, IBankManager bankManager,
            IRepository<BankTransaction> bankTransactionRepo, IRepository<UserTransaction> userTransactionRepo)
        {
            _loanRepo = loanRepo;
            _userRepo = userRepo;
            _userAccount = userAccount;
            _appSetting = appSetting;
            _httpClientFactory = httpClient;
            _configuration = configuration;
            _disbursementRepo = disbursementRepo;
            _contextAccessor = contextAccessor;
            _environment = environment;
            _bankManager = bankManager;
            _bankTransactionRepo = bankTransactionRepo;
            _userTransactionRepo = userTransactionRepo;
        }

        public async Task<Result<string>> Apply(LoanApplyForm form, string userName)
        {
            var user = _userRepo.AsQueryable().FirstOrDefault(k => k.UserName == userName);
            var account = _userAccount.AsQueryable().FirstOrDefault(n => n.UserId == user.Id);
            var interest = _appSetting.AsQueryable().FirstOrDefault(b => b.Id == ApplicationConstants.PercentInterest);

            // application fee of 1500 naira applies. If not available, deny application
            if (account.Balance <= 150000)
            {
                return new Result<string> { Succeeded = false, Error = "User does not have more than ₦1500 application fee." };
            }

            if (!string.IsNullOrEmpty(interest.Value) && ushort.Parse(interest.Value) > 0)
            {
                var loan = new Loan
                {
                    AmountRequested = form.Amount * 100,
                    DisbursementAccount = form.DisbursementAccount,
                    Tenure = form.Tenure,
                    UserAccountId = account.Id,
                    PecentInterest = ushort.Parse(interest.Value),
                    Status = LoanApplyStatus.Pending,
                    DateApplied = DateTime.Now,
                    BankNIPCode = form.BankCode,
                    Guarantors = form.Guarantors
                };

                AccountLookupObject lookup = new()
                {
                    account_number = form.DisbursementAccount,
                    nip_code = form.BankCode
                };

                var result = await _bankManager.AccountLookup(lookup);
                if (result.Succeeded == false)
                {
                    return new Result<string> { Succeeded = false, Error = result.Error };
                }

                // find guarantors
                if(!string.IsNullOrEmpty(form.Guarantors))
                {
                    var guarantors = form.Guarantors.Split(',');
                    foreach (var item in guarantors)
                    {
                        var guarantor = _userRepo.AsQueryable().FirstOrDefault(k => k.UserName == userName);
                        if (guarantor == null)
                        {
                            return new Result<string> { Succeeded = false, Error = $"The guarantor {item} could not be found in our system." };
                        }
                    }
                }
                
                // save documents
                foreach (var item in form.Documents)
                {
                    var location = $"{_environment.WebRootPath}/{Guid.NewGuid()}{item.FileName}";
                    var stream = new StreamWriter(location);
                    await item.OpenReadStream().CopyToAsync(stream.BaseStream);
                    stream.Close();
                    loan.SupportingDocuments += $"{_contextAccessor.HttpContext.Request.Host.Value}/{Path.GetFileName(location)}";
                }

                var userTransaction = new UserTransaction()
                {
                    Amount = 150000, // amount in kobo
                    UserAccountId = account.Id,
                    Type = "debit",
                    Date = DateTime.Now
                };
                _userTransactionRepo.Insert(userTransaction);

                account.Balance -= 150000;
                _loanRepo.Insert(loan);
                _loanRepo.SaveChanges();
                return new Result<string> { Succeeded = true, Error = "Loan Application successful." };
            }

            return new Result<string> { Succeeded = false, Error = "PI: Unable to proceed with loan application. Please contact the administrator." };
        }

        public Result<bool> Reject(long loanId, string comment, string rejectorUserName)
        {
            var approver = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == rejectorUserName);
            return new Result<bool>() { Succeeded = true };
        }

        public Result<bool> Approve(LoanApprovalForm form, string approverUserName)
        {
            Loan loan = _loanRepo.AsQueryable().FirstOrDefault(n => n.Id == form.Id);
            if (loan == null)
                throw new InvalidOperationException($"Cannot approve an inexistent loan with id: {form.Id}");

            var approver = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == approverUserName);
            loan.Status = LoanApplyStatus.Approved;
            loan.AmountGranted = form.AmountGranted.Value * 100;
            loan.ApproverId = approver.Id;
            loan.Comments = form.Comments;
            loan.DateApproved = DateTime.Now;

            _loanRepo.SaveChanges();
            return new Result<bool>() { Succeeded = true };
        }

        public async Task<Result<string>> DisburseLoan(long id)
        {
            var accountSetting = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.AccountId) ?? throw new NullReferenceException("Mono account has not been configured.");
            var secretKey = _appSetting.AsQueryable().FirstOrDefault(n => n.Id == ApplicationConstants.SecretKey) ?? throw new NullReferenceException("Mono account Secret Key has not been configured.");

            string url = $"{_configuration["MonoApi:BaseUrl"]}{_configuration["MonoApi:InitiatePayment"]}";

            Loan loan = _loanRepo.AsQueryable().FirstOrDefault(n => n.Id == id) ?? throw new InvalidOperationException($"Unable to find loan with id: {id}");
            LoanDisbursement disbursement = new()
            {
                LoanId = loan.Id,
                Amount = loan.AmountGranted,
                DisbursementAccount = loan.DisbursementAccount,
                Status = DisbursementStatus.Pending
            };

            _disbursementRepo.Insert(disbursement);
            _disbursementRepo.SaveChanges();

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("mono-sec-key", secretKey.Value);

            var payment = new InitiatePayment
            {
                Meta = new { reference = disbursement.Id },
                Amount = (loan.AmountGranted * 100).ToString(),
                Type = "onetime-debit",
                Description = $"Prudent Women Corporative Loan",
                Reference = disbursement.Id,
                Account = loan.DisbursementAccount,
                Redirect_url = $"{_contextAccessor.HttpContext.Request.Host.Value}/admin/Loans/disbursecomplete/"
            };

            var response = await httpClient.PostAsync(url, payment.SerializeObject());
            var data = await response.Content.ReadAsStringAsync();
            var jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (response.IsSuccessStatusCode)
            {
                var initResponse = JsonConvert.DeserializeObject<ApiResponse<InititatePaymentResponse>>(data, jsonSettings);
                disbursement.PaymentGatewayReference = initResponse.Data.Id;
                _disbursementRepo.SaveChanges();

                return new Result<string>()
                {
                    Succeeded = true,
                    Data = initResponse.Data.Payment_link
                };
            }

            var nn = JsonConvert.DeserializeObject<ApiResponse<InititatePaymentResponse>>(data, jsonSettings);
            return new Result<string>()
            {
                Succeeded = false,
                Error = nn.Message
            };
        }
    }
}
