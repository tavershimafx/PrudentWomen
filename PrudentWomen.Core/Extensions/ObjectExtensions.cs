using System.Net.Mime;
using System.Text;

namespace PrudentWomen.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static StringContent SerializeObject(this object value)
        {
            return new StringContent(System.Text.Json.JsonSerializer.Serialize(value), Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }
}
