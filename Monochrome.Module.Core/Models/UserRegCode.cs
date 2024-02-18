
namespace Monochrome.Module.Core.Models
{
    public class UserRegCode: BaseModel<string>
    {
        public UserRegCode()
        {
            Id = GenerateCode();
        }

        public static string GenerateCode()
        {
            Random rand = new();
            string[] strs = { "TSK", "TSP", "NTP" };

            return $"{strs[rand.Next(0, 2)]}-{rand.Next(10000, 100_000)}";
        }

        public bool IsUsed { get; set; }
    }
}