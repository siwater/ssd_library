using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers
{
    [Authorize]
    public class CreateController : Controller
    {
        private IDesktopManager mgr
        {
            get {
                return HttpContext.Session["IDesktopManager"] as IDesktopManager;
                }
        }

        //
        // GET: /Create/
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                return View(mgr.ListDesktopOfferings());
            }
            catch (System.Exception ex)
            {
                //CtxTrace.TraceError(ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        // POST: /Create/
        [HttpPost]
        public ActionResult Index(string serviceOfferingId, string button)
        {
            if (button == "Submit")
            {
                if (serviceOfferingId == null)
                {
                    return this.Index();
                }
                try
                {
                    var newDesktop = mgr.CreateDesktop(serviceOfferingId);
                }
                catch (System.Exception ex)
                {
                    //CtxTrace.TraceWarning(ex.Message);
                    ViewBag.ErrorMessage = ex.Message;
                    return View("Error");
                }
            }

            // TODO: Where is the error handling should create fail?
            return RedirectToAction("Index", "Manage"); 
        }
    }
}
