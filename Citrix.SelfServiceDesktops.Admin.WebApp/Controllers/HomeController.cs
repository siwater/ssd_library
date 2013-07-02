using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Citrix.SelfServiceDesktops.Admin;
using Citrix.SelfServiceDesktops.DesktopModel;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;

namespace Citrix.SelfServiceDesktops.Admin.WebApp.Controllers {

    public class HomeController : Controller {

        private const string ConfigStoreIndex = "config-store";
      
        public ActionResult Index() {

            return View();
        }

        public ActionResult ViewDesktopOfferings() {

            return View(ConfigStore.DesktopOfferings);
        }

        [HttpPost]
        public ActionResult New() {
            return View("Edit", new DesktopOfferingElement() { Name = "New desktop offering" });
        }

        [HttpPost]
        public ActionResult Edit(string identifier) {         
            IDesktopOffering offering = ConfigStore.DesktopOfferings.Where(o => o.Name == identifier).First();
            return View(offering);
        }

        [HttpPost]
        public ActionResult EndEdit(DesktopOfferingElement item) {
            IDesktopOffering offering = ConfigStore.DesktopOfferings.Where(o => o.Name == item.Name).FirstOrDefault();
            if (offering == null) {
                ConfigStore.AddDesktopOffering(item);
            } else {
                ConfigStore.ReplaceDesktopOffering(item);
            }
            //ConfigStore.Save();
            return RedirectToAction("ViewDesktopOfferings");
        }
        

        [HttpPost]
        public ActionResult Delete(string identifier) {
            ConfigStore.DeleteDesktopOffering(identifier);
            return RedirectToAction("ViewDesktopOfferings");
        }

        #region Private Methods

        private ConfigurationStore ConfigStore {
            get {
                ConfigurationStore store = HttpContext.Session[ConfigStoreIndex] as ConfigurationStore;
                if (store == null) {
                    store = ConfigurationStore.Open();
                    HttpContext.Session[ConfigStoreIndex] = store;
                }
                return store;
            }
        }

        #endregion
    }
}
