using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace Monochrome.Module.Core.Extensions
{
    /// <summary>
    /// A base controller for all api controllers
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
       
    }

    /// <summary>
    /// A base controller for all Mvc controllers
    /// </summary>
    public abstract class MvcBaseController : Controller
    {
        public void AddIdentityErrors(IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("Errors", error.Description);
            }
        }
    }
}