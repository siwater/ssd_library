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

namespace Citrix.SelfServiceDesktops.WebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        public const string ErrorMessageKey = "AuthError";
       
        /// <summary>
        /// GET: /Account/Login[/?[username=admin&][password=password&][ssoSessionkey=long_string]
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
            ControllerUtilities.CheckForNoCreate(this);
            ViewBag.ReturnUrl = returnUrl;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                return this.Login(new LoginModel() { Password = password, UserName = username }, returnUrl);
            }
            else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(sessionkey) && !string.IsNullOrEmpty(jsessionid))
            {
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
                        else if (!string.IsNullOrEmpty(model.SessionKey) && !string.IsNullOrEmpty(model.JSessionId)) 
                        {
                            mgr = factory.CreateManager(model.UserName, model.SessionKey, model.JSessionId, string.Empty);
                        } 
                        else 
                        {
                            ModelState.AddModelError(ErrorMessageKey, "Please enter valid credentials");
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
                        string message = ex.Message;
                        CloudStack.SDK.CloudStackException csex = ex as CloudStack.SDK.CloudStackException;
                        if (csex != null) {
                            message = csex.APIErrorResult.ErrorText;
                        }
                        if (InvalidCredentials(csex)) {
                            // Don't log full exception as it may contain password/session credentials (needs a fix in CSSDK)
                            string msg = String.Format("Invalid credentials entered. for user {0}", model.UserName);
                            CtxTrace.TraceError(msg);
                        } else {
                            CtxTrace.TraceError(ex);  
                        }
                        ModelState.AddModelError(ErrorMessageKey, message);
                    }
                }
            }
            // If we got this far, something failed, redisplay form       
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

        /// <summary>
        ///  Determine if an exception is as a result of the user entering invalid credential
        /// </summary>
        /// <param name="csex"></param>
        /// <returns></returns>
        private bool InvalidCredentials(CloudStack.SDK.CloudStackException csex) {
            return (csex != null) && (csex.APIErrorResult.ErrorCode == "531");
        }
        #endregion
    }
}
