using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.Admin.WebApp.Controllers {

    public class HomeController : Controller {

      
        public ActionResult Index() {

            return View();
        }

        public ActionResult ViewDesktops() {
            return View();
        }

        public ActionResult Edit(int item) {
            return View();
        }
    }
}
