using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Citrix.SelfServiceDesktops.DesktopModel;
using WebMatrix.WebData;
using System.Security.Principal;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers
{
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
        [BasicAuthenticationAttribute]
        public ActionResult Index()
        {
            CheckForNoCreate();
            ViewBag.ReceiverUrl = mgr.BrokerUrl.ToString();
            ViewBag.User = HttpContext.User.Identity.Name;
            return View(mgr.ListDesktops());
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
        [BasicAuthenticationAttribute]
        [HttpPost]
        public ActionResult Restart(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            mgr.RestartDesktop(name);
            return View("Index", mgr.ListDesktops());
        }

        //
        // POST: /Manage/Start?name=foo
        [BasicAuthenticationAttribute]
        [HttpPost]
        public ActionResult Start(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            mgr.StartDesktop(name);
            return View("Index", mgr.ListDesktops());
        }

        //
        // POST: /Manage/Stop?name=foo
        [BasicAuthenticationAttribute]
        [HttpPost]
        public ActionResult Stop(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            mgr.StopDesktop(name);
            return View("Index", mgr.ListDesktops());
        }

        //
        // POST: /Manage/Delete?name=foo
        // 
        [BasicAuthenticationAttribute]
        [HttpPost]
        public ActionResult Delete(string name)
        {
            CheckForNoCreate();
            ViewBag.User = HttpContext.User.Identity.Name;
            mgr.DestroyDesktop(name);
            return View("Index", mgr.ListDesktops());
        }

        // POST: /Manage/LoggedOff
        [HttpPost]
        public ActionResult LoggedOff()
        {
            CheckForNoCreate();
            // HttpContext set on Request basis
            if (HttpContext.Session["clear_logon"] != null)
            {
                HttpContext.Session.Abandon();
                return RedirectToAction("Index", "Manage");
           }

            // Revoke credentials
            HttpContext.Session.Add("clear_logon", "true");
            return new HttpBasicAuthenticationUnauthorizedResult();
        }
    }
}
