/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers {
    [Authorize]
    public class CreateController : Controller {
        private IDesktopManager mgr {
            get {
                return HttpContext.Session["IDesktopManager"] as IDesktopManager;
            }
        }

        //
        // GET: /Create/
        [AllowAnonymous]
        public ActionResult Index() {
            if (!HttpContext.User.Identity.IsAuthenticated) {
                return RedirectToAction("Login", "Account");
            }
            try {
                return View(mgr.ListDesktopOfferings());
            } catch (System.Exception ex) {
                CtxTrace.TraceError(ex);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        // POST: /Create/
        [HttpPost]
        public ActionResult Index(string serviceOfferingIdName, string button) {
            ControllerUtilities.CheckForNoCreate(this);
            System.Web.Routing.RouteValueDictionary route = new System.Web.Routing.RouteValueDictionary();
            if (button == "Submit") {
                if (serviceOfferingIdName == null) {
                    return this.Index();
                }
                try {
                    var newDesktop = mgr.CreateDesktop(serviceOfferingIdName);

                    // For CPBM plugin return a simple view with VM Id visible
                    if (ViewBag.NoLinks != null) {
                        return View("CreateComplete", newDesktop);
                    }
                    route.Add("newId", newDesktop.Id);
                } catch (System.Exception ex) {
                    CtxTrace.TraceError(ex);
                    ViewBag.ErrorMessage = ex.Message;
                    return View("Error");
                }
            }
            return RedirectToAction("Index", "Manage", route);
        }

        [AllowAnonymous]
        public ActionResult CreateComplete(IDesktop newDesktop) {
            return View(newDesktop);
        }
    }
}
