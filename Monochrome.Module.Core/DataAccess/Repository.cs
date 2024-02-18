using Microsoft.Extensions.Logging;
using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.DataAccess
{
    public class Repository<T> : Repository<long, T>, IRepository<T> where T : BaseModel<long>
    {
        public Repository(ApplicationDbContext context, ILogger<Repository<long, T>> logger) : base(context, logger)
        {

        }
    }
}