using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monochrome.Module.Core.Models
{
    [Index(nameof(CreatedById), IsUnique = false)]
    [Index(nameof(UpdatedById), IsUnique = false)]
    public class Role : IdentityRole, IEntityDefinition<string>
    {
        public Role(): base() { }
        public Role(string roleName) : base(roleName) { }

        [ForeignKey(nameof(CreatedBy))]
        public string CreatedById { get; set; }

        public User CreatedBy { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public string UpdatedById { get; set; }

        public User UpdatedBy { get; set; }

        public DateTimeOffset? LastUpdated { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}