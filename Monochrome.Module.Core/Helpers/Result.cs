namespace Monochrome.Module.Core.Helpers
{
    public class Result<T>
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
    }
}
