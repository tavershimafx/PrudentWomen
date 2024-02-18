using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Monochrome.Module.Core.DataAccess
{
    public class Repository<Key, T> : IRepository<Key, T> where T : class, IEntityDefinition<Key>
    {
        private readonly ILogger<Repository<Key, T>> _logger;
        public Repository(ApplicationDbContext context, ILogger<Repository<Key, T>> logger)
        {
            Context = context;
            DbSet = Context.Set<T>();
            _logger = logger;
        }

        protected ApplicationDbContext Context { get; }

        protected DbSet<T> DbSet { get; }

        public async Task InsertAsync(T model)
        {
            await DbSet.AddAsync(model);
        }

        public void Insert(T model)
        {
            DbSet.Add(model);
        }

        public async Task InsertOrUpdate(T model)
        {
            var entity = Context.Entry(model);
            if (entity != null)
            {
                Update(model);
            }
            else
            {
                await InsertAsync(model);
            }
        }

        public async Task InsertRangeAsync(IEnumerable<T> model)
        {
            await DbSet.AddRangeAsync(model);
        }

        public void InsertRange(IEnumerable<T> model)
        {
            DbSet.AddRange(model);
        }

        public IDbContextTransaction BeginTransaction()
        {
            return Context.Database.BeginTransaction();
        }

        public DbTransactionInfo SaveChanges()
        {
            try
            {
                Context.SaveChanges();

                return new DbTransactionInfo { Succeeded = true };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new DbTransactionInfo
                {
                    Errors = new List<string> { ex.Message },
                    Succeeded = false
                };
            }
            catch (DbUpdateException ex)
            {
                return new DbTransactionInfo
                {
                    Errors = new List<string> { ex.Message },
                    Succeeded = false
                };
            }
        }

        public async Task<DbTransactionInfo> SaveChangesAsync()
        {
            try
            {
                await Context.SaveChangesAsync();

                return new DbTransactionInfo { Succeeded = true };
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError(e.Message);
                return new DbTransactionInfo
                {
                    Errors = new List<string> { "An error occured while attempting to update the record you requested. " +
                    "Another user updated the record since you last retrieved it. Try get a fresh copy of the record " +
                    "and try again." },
                    Succeeded = false
                };
            }
            catch (DbUpdateException e)
            {
                _logger.LogCritical(e.Message);
                return new DbTransactionInfo
                {
                    Errors = new List<string> { "An error occured while persisting your changes to the backing store." },
                    Succeeded = false
                };
            }
        }

        public IQueryable<T> AsQueryable()
        {
            return DbSet;
        }

        public async Task DeleteAsync(Key key)
        {
            var entity = await DbSet.FindAsync(key);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }

        public Task DeleteAsync(Func<T, bool> expression)
        {
            var entities = DbSet.Where(expression);
            foreach (var entity in entities)
            {
                DbSet.Remove(entity);
            }
            return Task.CompletedTask;
        }

        public void DeleteRange(IEnumerable<T> models)
        {
            DbSet.RemoveRange(models);
        }

        public void Delete(T model)
        {
            if (model != null)
                DbSet.Remove(model);
        }

        public void Update(T model)
        {
            if (model != null)
                DbSet.Update(model);
        }
    }
}