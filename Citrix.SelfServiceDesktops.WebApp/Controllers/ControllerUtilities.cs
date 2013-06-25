using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers {

    public class ControllerUtilities {

        public const string NoLinks = "nolinks";

        public static void CheckForNoCreate(Controller controller)
        {
            if (!controller.Request.QueryString.AllKeys.Contains(NoLinks) && controller.HttpContext.Session[NoLinks] == null)
            {
                return;
            }
            controller.ViewBag.NoLinks = true;
            controller.HttpContext.Session.Add(NoLinks, "true");
        }
    }
}