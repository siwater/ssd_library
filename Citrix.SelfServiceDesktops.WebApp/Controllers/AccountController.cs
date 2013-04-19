using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using System.Security.Principal;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.Models;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                IDesktopManagerFactory factory = System.Web.HttpContext.Current.Application.Get("IDesktopManagerFactory") as IDesktopManagerFactory;
                if (factory != null)
                {
                    try
                    {
                        IDesktopManager mgr = factory.CreateManager(model.UserName, model.Password);
                        if (mgr != null)
                        {
                            HttpContext.Session.Add("IDesktopManager", mgr);
                            FormsAuthentication.SetAuthCookie(model.UserName, false);
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ////CtxTrace.TraceError(ex.Message);
                        ViewBag.ErrorMessage = ex.Message;
                        return View("Error"); // Exceptions thrown when authentication fails, but we ignore to avoid giving away details about the cloudstack command.
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            HttpContext.Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Manage");
            }
        }

        #endregion
    }
}
