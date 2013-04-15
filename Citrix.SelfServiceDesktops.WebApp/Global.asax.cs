using Citrix.SelfServiceDesktops.DesktopLibrary;
using Citrix.SelfServiceDesktops.DesktopModel;
using Citrix.SelfServiceDesktops.MockDesktopLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Citrix.SelfServiceDesktops.WebApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // Add the DesktopManagerFactory
//            IDesktopManagerFactory factory = new DesktopManagerFactory();
            IDesktopManagerFactory factory = new MockDesktopManagerFactory();
            HttpContext.Current.Application.Add("IDesktopManagerFactory", factory);
        }
    }
}