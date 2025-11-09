using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hotels.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetInt32("VaiTro");

            if (role == null || role == 1)
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
            }

            base.OnActionExecuting(context);
        }
    }
}
