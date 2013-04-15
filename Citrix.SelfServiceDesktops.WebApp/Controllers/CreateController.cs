using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers
{
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
        [BasicAuthenticationAttribute]
        public ActionResult Index()
        {
            return View(mgr.ListDesktopOfferings());
        }

        // POST: /Create/
        [HttpPost]
        public ActionResult Index(string serviceOfferingId, string button)
        {
            if (serviceOfferingId == null)
            {
                return this.Index();
            }

            if (button == "Submit")
            {
                var newDesktop = mgr.CreateDesktop(serviceOfferingId);
            }

            // TODO: Where is the error handling should create fail?
            return RedirectToAction("Index", "Manage"); 
        }
    }
}
