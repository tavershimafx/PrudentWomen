using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Monochrome.Module.Core.Models;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Monochrome.Module.Core.Areas.Admin.ViewModels
{
    public class UserListItemVm
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string[] Roles { get; set; }

        public bool EmailConfirmed { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UserStatus Status { get; set; }
    }
}