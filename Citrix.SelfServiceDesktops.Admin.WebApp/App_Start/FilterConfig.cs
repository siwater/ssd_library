using System.Web;
using System.Web.Mvc;

namespace Citrix.SelfServiceDesktops.Admin.WebApp {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}