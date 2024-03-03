using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monochrome.Module.Core.Models
{
    [Index(nameof(CreatedById), IsUnique = false)]
    [Index(nameof(UpdatedById), IsUnique = false)]
    public class User : IdentityUser, IEntityDefinition<string>
    {
        public User() :base()
        {
            PrudentNumber = GenerateUserNumber();
        }

        public User(string userName) : base(userName)
        {
            PrudentNumber = GenerateUserNumber();
        }

        public static string GenerateUserNumber()
        {
            Random rand = new();
            return $"PWC{rand.Next(10000, 100_000)}";
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicture { get; set; }

        [NotMapped]
        public string PrudentNumber { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public string CreatedById { get; set; }

        public User CreatedBy { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public string UpdatedById { get; set; }

        public User UpdatedBy { get; set; }

        public DateTimeOffset? LastUpdated { get; set; }

        public UserStatus Status { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}