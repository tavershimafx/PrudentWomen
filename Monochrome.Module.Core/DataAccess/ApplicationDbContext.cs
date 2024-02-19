using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Monochrome.Module.Core.Extensions;
using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        // Note:
        // In order to add migration, you need to remove the IHttpContextAccessor from the
        // constructor away after that, return it back else audit columns will not be updated
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor contextAccessor) : base(options)
        {
            _contextAccessor = contextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("Mono.Users");
            builder.Entity<Role>().ToTable("Mono.Roles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("Mono.UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("Mono.UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("Mono.RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("Mono.UserTokens");
            builder.Entity<UserRole>(x =>
            {
                x.HasOne(k => k.User)
                .WithMany(k => k.UserRoles)
                .HasForeignKey(x => x.UserId);

                x.HasOne(k => k.Role)
                .WithMany(k => k.UserRoles)
                .HasForeignKey(x => x.RoleId);

                x.ToTable("Mono.UserRoles");
            });

            builder.Entity<UserRegCode>().ToTable("Prudent.RegCodes");
            builder.Entity<BankTransaction>().ToTable("Prudent.BankTransactions");
            builder.Entity<UserAccount>().ToTable("Prudent.UserAccounts");
            builder.Entity<UserTransaction>().ToTable("Prudent.UserTransactions");
            builder.Entity<Loan>().ToTable("Prudent.Loans");
            builder.Entity<ApplicationSetting>().ToTable("Prudent.ApplicationSettings");

        }

        #region DbContext Overrides

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            base.OnConfiguring(optionsBuilder);
        }

        public override int SaveChanges()
        {
            UpdateColumns();
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException($"An error occured saving the current changes. Another user might have updated the record " +
                        $"since you last retrieved it. Try retrieve the record and attempt the operation again.");
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateColumns();
            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException($"An error occured saving the current changes. Another user might have updated the record " +
                        $"since you last retrieved it. Try retrieve the record and attempt the operation again.");
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateColumns();
            try
            {
                return base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException($"An error occured saving the current changes. Another user might have updated the record " +
                        $"since you last retrieved it. Try retrieve the record and attempt the operation again.");
            }
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateColumns();
            try
            {
                return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException($"An error occured saving the current changes. Another user might have updated the record " +
                        $"since you last retrieved it. Try retrieve the record and attempt the operation again.");
            }
        }

        #endregion

        private void UpdateColumns()
        {
            var entries = ChangeTracker.Entries().Where(n => n.State == EntityState.Added || n.State == EntityState.Modified);

            User user = null;
            if (_contextAccessor?.HttpContext != null && _contextAccessor.HttpContext.User != null && _contextAccessor.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                user = _contextAccessor.GetCurrentUser().GetAwaiter().GetResult();
            }

            foreach (var item in entries)
            {
                var lastPropInfo = item.Entity.GetType().GetProperty(nameof(BaseModel.LastUpdated));
                var lastPropUser = item.Entity.GetType().GetProperty(nameof(BaseModel.UpdatedById));

                lastPropInfo?.SetValue(item.Entity, new DateTimeOffset(new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
                lastPropUser?.SetValue(item.Entity, user?.Id);

                if (item.State == EntityState.Added)
                {
                    var newPropUser = item.Entity.GetType().GetProperty(nameof(BaseModel.CreatedById));
                    var newPropInfo = item.Entity.GetType().GetProperty(nameof(BaseModel.DateCreated));
                    var concurrencyInfo = item.Entity.GetType().GetProperty(nameof(BaseModel.ConcurrencyStamp));

                    newPropInfo?.SetValue(item.Entity, new DateTimeOffset(new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
                    concurrencyInfo?.SetValue(item.Entity, Guid.NewGuid().ToString("N").ToUpper());
                    newPropUser?.SetValue(item.Entity, user?.Id);

                }
            }
        }
    }
}