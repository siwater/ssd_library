/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
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

        /// <summary>
        /// /Account/Login[/?[username=admin&][password=password&][ssoSessionkey=long_string]
        /// 
        /// e.g
        /// 
        /// /Account/Login
        /// 
        /// /Account/Login/?username=admin&password=password
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ssoSessionKey"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string username, string password, string sessionkey, string jsessionid )
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                return this.Login(new LoginModel() { Password = password, UserName = username }, returnUrl);
            }
            else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(sessionkey) && !string.IsNullOrEmpty(jsessionid))
            {
                // TODO: revise to use SSO key
                return this.Login(new LoginModel() { UserName = username, JSessionId = jsessionid, SessionKey = sessionkey }, returnUrl);
            }
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
                        IDesktopManager mgr = null;

                        if (!string.IsNullOrEmpty(model.Password))
                        {
                            mgr = factory.CreateManager(model.UserName, model.Password, string.Empty);
                        }
                        else
                        {
                            mgr = factory.CreateManager(model.UserName, model.SessionKey, model.JSessionId, string.Empty);
                        }

                        if (mgr != null)
                        {
                            HttpContext.Session.Add("IDesktopManager", mgr);
                            FormsAuthentication.SetAuthCookie(model.UserName, false);
                            return RedirectToLocal(null); // ignore returnUrl, always go to Manage
                        }
                    }
                    catch (System.Exception ex)
                    {
                        CtxTrace.TraceError(ex);
                        ViewBag.ErrorMessage = ex.Message;
                        // return View("Error"); // Exceptions thrown when authentication fails, but we ignore to avoid giving away details about the cloudstack command.
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

        // GET: /Account/LogOff
        [HttpGet]
        public ActionResult LogOff(string blankForOverloading)
        {
            return this.LogOff();
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
