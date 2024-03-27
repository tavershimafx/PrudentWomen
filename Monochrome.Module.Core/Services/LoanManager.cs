using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Helpers;
using Monochrome.Module.Core.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Monochrome.Module.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Monochrome.Module.Core.Services.Email;

namespace Monochrome.Module.Core.Services
{
    public class LoanManager: ILoanManager
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<LoanRepaymentHistory> _repaymentRepo;
        private readonly IRepository<LoanGuarantor> _guarantorRepo;
        private readonly IRepository<string, User> _userRepo;
        private readonly IRepository<string, LoanDisbursement> _disbursementRepo;
        private readonly IRepository<string, ApplicationSetting> _appSetting;
        private readonly IBankManager _bankManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _emailSender;

        public LoanManager(IRepository<Loan> loanRepo, IRepository<string, User> userRepo,
            IRepository<string, ApplicationSetting> appSetting,
            IHttpClientFactory httpClient, IConfiguration configuration, IRepository<string, LoanDisbursement> disbursementRepo,
            IHttpContextAccessor contextAccessor, IWebHostEnvironment environment, IBankManager bankManager,
            IRepository<LoanRepaymentHistory> repaymentRepo,
            IRepository<LoanGuarantor> guarantorRepo, IEmailSender emailSender)
        {
            _loanRepo = loanRepo;
            _userRepo = userRepo;
            _appSetting = appSetting;
            _httpClientFactory = httpClient;
            _configuration = configuration;
            _disbursementRepo = disbursementRepo;
            _contextAccessor = contextAccessor;
            _environment = environment;
            _bankManager = bankManager;
            _guarantorRepo = guarantorRepo;
            _emailSender = emailSender;
            _repaymentRepo = repaymentRepo;
        }

        public async Task<Result<string>> Apply(LoanApplyForm form, string userName)
        {
            var user = _userRepo.AsQueryable().FirstOrDefault(k => k.UserName == userName);
            var account = _bankManager.GetAccount(user.Id);
            var interest = _appSetting.AsQueryable().FirstOrDefault(b => b.Id == ApplicationConstants.PercentInterest);
            var loanFeeSetting = _appSetting.AsQueryable().FirstOrDefault(b => b.Id == ApplicationConstants.LoanApplicationFee);
            var loanFee = decimal.Parse(loanFeeSetting.Value) * 100;

            // Check for loan application fee. If not available, deny application
            if (account.Balance <= loanFee)
            {
                return new Result<string> { Succeeded = false, Error = $"User does not have more than ₦{loanFee/100} application fee." };
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
                    Status = ApplicationStatus.Pending,
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
                var guarantorList = new List<User>();
                if(!string.IsNullOrEmpty(form.Guarantors))
                {
                    var guarantors = form.Guarantors.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in guarantors)
                    {
                        var guarantor = _userRepo.AsQueryable().FirstOrDefault(k => k.UserName == userName);
                        if (guarantor == null)
                        {
                            return new Result<string> { Succeeded = false, Error = $"The guarantor {item} could not be found in our system." };
                        }

                        guarantorList.Add(guarantor);
                    }

                    _guarantorRepo.InsertRange(guarantorList.Select(n => new LoanGuarantor
                    {
                        Loan = loan,
                        UserId = n.Id,
                        AmountRequested = loan.AmountRequested,
                        Status = ApplicationStatus.Pending
                    }));
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

                _bankManager.CreditSuperAccount(account.Id, loanFee, "Loan application fee");

                try
                {
                    _loanRepo.Insert(loan);
                    _loanRepo.SaveChanges();
                }
                catch (Exception)
                {
                    _bankManager.DebitSuperAccount(account.Id, loanFee, "Loan application fee reversal");
                    return new Result<string> { Succeeded = false, Error = "Loan Application successful." };
                }

                // send email to loan guarantors
                if (guarantorList.Any())
                {
                    foreach (var guarantor in guarantorList)
                    {
                        await _emailSender.SendEmailAsync(guarantor.Email, "Request to be gurant a loan",
                            $"A user has requested for you to be his guarantor on Prudent Woment Coorporative " +
                            $"portal. Please visit your dashboard to review this request.");

                    }
                }
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
                throw new InvalidOperationException($"Cannot approve an in-existent loan with id: {form.Id}");

            var approver = _userRepo.AsQueryable().FirstOrDefault(n => n.UserName == approverUserName);
            loan.Status = ApplicationStatus.Approved;
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

        public IEnumerable<LoanList> GetUnpaidLoans()
        {
            var loans = _loanRepo.AsQueryable()
                .AsNoTracking()
                .Where(k => k.Status == ApplicationStatus.Approved && k.Repaid == false);

            return loans.Select(n => new LoanList
            {
                Id = n.Id,
                AmountRequested = n.AmountRequested,
                DateApplied = n.DateApplied,
                DateApproved = n.DateApproved,
                UserAccount = n.UserAccount.User.UserName,
                Disbursed = n.DateDisbursed != null,
                Repaid = n.Repaid,
                Status = n.Status,
                Tenure = n.Tenure
            }).AsEnumerable();
        }

        public Result<string> AddRepayment(long transactionId, long loanId)
        {
            Loan loan = _loanRepo.AsQueryable().FirstOrDefault(n => n.Id == loanId);
            var transaction = _bankManager.GetTransaction(transactionId);

            if (loan == null || transaction == null)
            {
                throw new InvalidOperationException("Cannot continue with the operation because either the loan Id " +
                    "presented was not found in the tracking collection or the transaction Id is invalid.");
            }

            if (transaction.IsIdentified)
            {
                return new Result<string>() { Succeeded = false, Error = "Cannot use this transaction for a loan repayment" +
                    " because it has already been credited to a user's account." };
            }

            var prevRepayments = _repaymentRepo.AsQueryable().Where(k => k.LoanId == loan.Id);
            decimal totalRepaid = 0;
            if (prevRepayments.Any())
            {
                totalRepaid += prevRepayments.Sum(b => b.Amount);
            }

            if ((totalRepaid + transaction.Amount) == loan.AmountGranted)
            {
                loan.Repaid = true;
            }

            var history = new LoanRepaymentHistory
            {
                LoanId = loan.Id,
                Amount = transaction.Amount
            };

            _repaymentRepo.Insert(history);
            _repaymentRepo.SaveChanges();

            var interest = (loan.PecentInterest / 100) * loan.AmountGranted;

            _bankManager.CreditSuperAccount(loan.UserAccountId, interest, "Interest on loan", "Debit for loan interest");
            return new Result<string>() { Succeeded = true };
        }
    }
}
