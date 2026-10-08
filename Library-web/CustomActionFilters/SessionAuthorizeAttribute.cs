using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace library_web.CustomActionFilters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}