using Microsoft.AspNetCore.Http;

namespace Monochrome.Module.Core.Extensions
{
    public static class HttpRequestExtentions
    {
        public static string GetFullHostingUrlRoot(this HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host.Value}";
        }
    }
}