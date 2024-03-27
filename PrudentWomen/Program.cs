using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Events;
using Monochrome.Module.Core.Models;
using Monochrome.Module.Core.Services.Email;
using Monochrome.Module.Core.Services;
using Serilog;
using System.Reflection;
using Hangfire;

namespace PrudentWomen
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Logging setup
            builder.Services.AddLogging();

            builder.Logging.AddSerilog();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            #endregion

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName));
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<User, Role>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var mvcBuilder = builder.Services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .AddRazorRuntimeCompilation();

            #region HangFire services

            builder.Services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddHangfireServer();

            #endregion

            #region Services

            builder.Services.AddHttpClient();
            //builder.Services.AddScoped<MediatR.ServiceFactory>(p => p.GetService);
            builder.Services.AddScoped<IMediator, Mediator>();
            //builder.Services.AddMediatR(config =>
            //{
                
            //});
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IBankManager, BankManager>();
            builder.Services.AddScoped<ILoanManager, LoanManager>();
            builder.Services.AddScoped<INotificationHandler<UserCreated>, UserCreatedHandler>();

            #endregion

            #region Identity configurations
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-_@1234567890";
                options.User.RequireUniqueEmail = true;
            });

            #endregion

            #region Authentication and Authorization

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Authentication/Auth/Login";
                options.AccessDeniedPath = "/Authentication/Auth/AccessDenied";
            });

            #endregion

            CreateAdminAndRoles(builder.Services).Wait();
            //RegisterHangFireJobs();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire");

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllerRoute(
                    name: "default",
                    pattern: "{area:exists}/{controller=Auth}/{action=Login}/{id?}");

                endpoint.MapAreaControllerRoute(
                    name: "areas",
                    areaName: "Authentication",
                    pattern: "{controller=Auth}/{action=Login}/{id?}");

                endpoint.MapControllerRoute(
                    name: "controllers",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.Run();
        }

        public static async Task<bool> CreateAdminAndRoles(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider() as IServiceProvider;
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            await roleManager.CreateAsync(new Role("Admin"));
            await roleManager.CreateAsync(new Role("SuperAdmin"));
            await roleManager.CreateAsync(new Role("Customer"));

            User adminUser = new()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "Admin",
                Email = "admin@prudentwomen.org",
                EmailConfirmed = true,
                Status = UserStatus.Active
            };

            User superUser = new()
            {
                FirstName = "Super",
                LastName = "Admin",
                UserName = "SuperAdmin",
                Email = "postmaster@prudentwomen.org",
                EmailConfirmed = true,
                Status = UserStatus.Active
            };

            if ((await userManager.CreateAsync(adminUser, "LiveOceanStreams")).Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            if ((await userManager.CreateAsync(superUser, "DukeOfMilan")).Succeeded)
            {
                await userManager.AddToRoleAsync(superUser, "SuperAdmin");
            }

            CreateSystemAccount(services, superUser.Id);

            var appSetting = serviceProvider.GetRequiredService<IRepository<string, ApplicationSetting>>();
            
            // account id
            if (appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.AccountId) == null)
            {
                appSetting.Insert(new ApplicationSetting() { Id = ApplicationConstants.AccountId, Value = "" });
            }
            
            // tax percent
            if (appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.PercentInterest) == null)
            {
                appSetting.Insert(new ApplicationSetting() { Id = ApplicationConstants.PercentInterest, Value = "30" });
            }

            // secret key
            if (appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.SecretKey) == null)
            {
                appSetting.Insert(new ApplicationSetting() { Id = ApplicationConstants.SecretKey, Value = "" });
            }
            
            // public key
            if (appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.PublicKey) == null)
            {
                appSetting.Insert(new ApplicationSetting() { Id = ApplicationConstants.PublicKey, Value = "" });
            }

            // Loan application fee
            if (appSetting.AsQueryable().FirstOrDefault(k => k.Id == ApplicationConstants.LoanApplicationFee) == null)
            {
                appSetting.Insert(new ApplicationSetting() { Id = ApplicationConstants.LoanApplicationFee, Value = "1500" });
            }
            appSetting.SaveChanges();
            
            return true;
        }

        // create a default account to store money such as loan interest, loan application fee and others
        // charged to the account of users
        private static void CreateSystemAccount(IServiceCollection services, string userId)
        {
            var serviceProvider = services.BuildServiceProvider() as IServiceProvider;
            var _userAccount = serviceProvider.GetRequiredService<IRepository<UserAccount>>();
            var account = new UserAccount
            {
                UserId = userId,
            };

            _userAccount.Insert(account);
            _userAccount.SaveChanges();
        }

        private static void RegisterHangFireJobs()
        {
            RecurringJob.AddOrUpdate<IBankManager>("Transaction Sync", p => p.SynchronizeWithMono(default, default, true), Cron.Hourly());
        }
    }
}