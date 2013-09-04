using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Citrix.SelfServiceDesktops.WebApp.Controllers {

    public class ControllerUtilities {

        /// <summary>
        /// Query string "nolinks=true" to suppress the menu in the shared header (see _Layout.cshtml)
        /// </summary>
        public const string NoLinks = "nolinks";

        /// <summary>
        /// If query string "displayvmid=true" is specified, then the Create page will return the VM Id of a new virtual machine
        /// rather than redirecting the user back to the Manage page.
        /// </summary>
        public const string DisplayVmId = "displayvmid";

        public static void CheckFlags(Controller controller)
        {
            if (controller.Request.QueryString.AllKeys.Contains(NoLinks) || controller.HttpContext.Session[NoLinks] != null) {
                controller.ViewBag.NoLinks = true;
                controller.HttpContext.Session.Add(NoLinks, "true");
            }

            if (controller.Request.QueryString.AllKeys.Contains(DisplayVmId) || controller.HttpContext.Session[DisplayVmId] != null) {
                controller.ViewBag.DisplayVmId = true;
                controller.HttpContext.Session.Add(DisplayVmId, "true");
            }
        }
    }
}