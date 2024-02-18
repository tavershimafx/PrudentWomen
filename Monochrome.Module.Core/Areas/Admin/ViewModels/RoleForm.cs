using System.ComponentModel.DataAnnotations;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class RoleForm
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}