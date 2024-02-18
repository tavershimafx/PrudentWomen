using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public abstract class UserFormBase
    {
        public string Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        public string[] RoleIds { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
    public class UserCreateForm : UserFormBase
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserUpdateForm : UserFormBase
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}