using Monochrome.Module.Core.Models;

namespace Monochrome.Module.Core.DataAccess
{
    /// <summary>
    /// A base repository class for all data models with key type of long
    /// </summary>
    /// <typeparam name="T">The data model or object</typeparam>
    public interface IRepository<T> : IRepository<long, T> where T : BaseModel<long>
    {

    }
}