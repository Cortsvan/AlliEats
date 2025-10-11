using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ASI.Basecode.WebApp.Attributes
{
    public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");

            if (userRole != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}