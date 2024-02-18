namespace Monochrome.Module.Core.Models
{
    public class UserAccount : BaseModel
    {
        public decimal Balance { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTimeOffset LastTransaction { get; set; }
    }
}
