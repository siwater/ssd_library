﻿/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers
{

    [Authorize]
    public class ManageController : Controller
    {
        private IDesktopManager mgr
        {
            get
            {
                IDesktopManager result = HttpContext.Session["IDesktopManager"] as IDesktopManager;
                return result;
            }
        }
        //
        // GET: /Manage/
        // TODO:  Add Create URL
        [AllowAnonymous]
        public ActionResult Index(string newId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            ControllerUtilities.CheckFlags(this);
            try
            {
                ViewBag.ReceiverUrl = mgr.BrokerUrl.ToString();
                ViewBag.User = HttpContext.User.Identity.Name;

                return View(mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                CtxTrace.TraceError(ex);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Restart?name=foo
        [HttpPost]
        public ActionResult Restart(string identifier)
        {
            ControllerUtilities.CheckFlags(this);
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.RestartDesktop(identifier);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                CtxTrace.TraceError(ex);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Start?name=foo
        [HttpPost]
        public ActionResult Start(string identifier)
        {
            ControllerUtilities.CheckFlags(this);
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.StartDesktop(identifier);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                CtxTrace.TraceError(ex);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Stop?name=foo
        [HttpPost]
        public ActionResult Stop(string identifier)
        {
            ControllerUtilities.CheckFlags(this);
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.StopDesktop(identifier);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                CtxTrace.TraceError(ex);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Delete?name=foo
        // 
        [HttpPost]
        public ActionResult Delete(string identifier)
        {
            ControllerUtilities.CheckFlags(this);
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.DestroyDesktop(identifier);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                CtxTrace.TraceError(ex);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}
