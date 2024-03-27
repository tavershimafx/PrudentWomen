using Newtonsoft.Json;

namespace Monochrome.Module.Core.Models
{
    public class UserTransaction: BaseModel
    {
        public string Type { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Date { get; set; }
        public decimal Balance { get; set; }

        public long UserAccountId { get; set; }

        [JsonIgnore]
        public UserAccount UserAccount { get; set; }
    }
}
