using Monochrome.Module.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Monochrome.Module.Core.DataAccess;
using Monochrome.Module.Core.Models;
using Monochrome.Module.Core.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Monochrome.Module.Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/roles")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RolesController : MvcBaseController
    {
        private readonly IRepository<string, Role> _roleRepository;

        public RolesController(IRepository<string, Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpGet("list")]
        public IActionResult Index()
        {
            var roles = _roleRepository.AsQueryable()
                .Select(x => new RoleListItemVm
                {
                    Id = x.Id,
                    Name = x.Name
                });

            return View(roles);
        }

        [HttpGet("get-role/{id}")]
        public IActionResult GetRole(string id)
        {
            var role = _roleRepository.AsQueryable().FirstOrDefault(k => k.Id == id);
            if (role != null)
            {
                var model = new RoleForm
                {
                    Id = role.Id,
                    Name = role.Name,
                    ConcurrencyStamp = role.ConcurrencyStamp,
                };

                return Ok(model);
            }

            return BadRequest("Role not found.");
        }

        [HttpPost("create")]
        public IActionResult CreateRole([FromQuery] RoleForm model)
        {
            if (ModelState.IsValid)
            {
                var role = new Role { Name = model.Name };
                _roleRepository.Insert(role);
                _roleRepository.SaveChanges();
                return Ok(model);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("update")]
        public IActionResult UpdateRole([FromQuery] RoleForm model)
        {
            if (ModelState.IsValid)
            {
                var role = _roleRepository.AsQueryable().FirstOrDefault(k => k.Id == model.Id);
                if (role != null)
                {
                    role.Name = model.Name;

                    _roleRepository.SaveChanges();
                    return Ok("Role updated");
                }

                return BadRequest("Role not found.");
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("delete")]
        public IActionResult DeleteUser(string roleId)
        {
            var role = _roleRepository.AsQueryable().FirstOrDefault(x => x.Id == roleId);
            if (role != null)
            {
                _roleRepository.Delete(role);
                _roleRepository.SaveChanges();

                return Ok("Role Deleted");
            }

            return BadRequest("Role not found.");
        }
    }
}