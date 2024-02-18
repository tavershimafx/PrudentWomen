namespace Monochrome.Module.Core.Models
{
    public interface IEntityDefinition<T>
    {
        public T Id { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}