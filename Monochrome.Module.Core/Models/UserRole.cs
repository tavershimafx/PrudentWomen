using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monochrome.Module.Core.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public Role Role { get; set; }

        public User User { get; set; }
    }
}