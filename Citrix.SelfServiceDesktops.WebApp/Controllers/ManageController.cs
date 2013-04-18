﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Citrix.SelfServiceDesktops.DesktopModel;
using WebMatrix.WebData;
using System.Security.Principal;

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
        public ActionResult Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            CheckForNoCreate();
            try
            {
                ViewBag.ReceiverUrl = mgr.BrokerUrl.ToString();
                ViewBag.User = HttpContext.User.Identity.Name;

                return View(mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        private void CheckForNoCreate()
        {
            if (!Request.QueryString.AllKeys.Contains("nocreate") && HttpContext.Session["nocreate"] == null)
            {
                return;
            }
            ViewBag.NoCreate = true;
            HttpContext.Session.Add("nocreate", "true");
            return;
        }

        //
        // POST: /Manage/Restart?name=foo
        [HttpPost]
        public ActionResult Restart(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.RestartDesktop(name);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Start?name=foo
        [HttpPost]
        public ActionResult Start(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.StartDesktop(name);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Stop?name=foo
        [HttpPost]
        public ActionResult Stop(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.StopDesktop(name);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /Manage/Delete?name=foo
        // 
        [HttpPost]
        public ActionResult Delete(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            try
            {
                mgr.DestroyDesktop(name);
                return View("Index", mgr.ListDesktops());
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}