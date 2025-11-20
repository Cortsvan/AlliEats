using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ASI.Basecode.WebApp.Attributes
{
    public class CustomerOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (context.HttpContext.User.IsInRole("Admin"))
            {
                var tempData = context.HttpContext.RequestServices
                    .GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory)) as
                    Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory;

                var tempDataDict = tempData?.GetTempData(context.HttpContext);
                if (tempDataDict != null)
                {
                    tempDataDict["ErrorMessage"] = "Admins cannot access cart functionality.";
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");

            if (userRole == "Admin")
            {
                var tempData = context.HttpContext.RequestServices
                    .GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory)) as
                    Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory;

                var tempDataDict = tempData?.GetTempData(context.HttpContext);
                if (tempDataDict != null)
                {
                    tempDataDict["ErrorMessage"] = "Admins cannot access cart functionality.";
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}