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
            try {
                ConfigurationStore store = ConfigStore;
                return View();
            } catch (Exception e) {           
                return View("Error", e);
            }
        }

        public ActionResult ViewDesktopOfferings() {
            try {
                return View(ConfigStore.DesktopOfferings);
            } catch (Exception e) {
                ViewBag.Message = e.Message;
                return View("Index");
            }
        }

        public ActionResult ViewSettings() {
            try {
                return View(ConfigStore);
            } catch (Exception e) {
                ViewBag.Message = e.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult EndEditSettings(DesktopServiceConfiguration item) {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult NewDesktopOffering() {
            return View("EditDesktopOffering", new DesktopOfferingElement() { Name = "New desktop offering" });
        }

        [HttpPost]
        public ActionResult EditDesktopOffering(string identifier) {         
            IDesktopOffering offering = ConfigStore.DesktopOfferings.Where(o => o.Name == identifier).First();
            return View(offering);
        }

        [HttpPost]
        public ActionResult EndEditDesktopOffering(DesktopOfferingElement item) {
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
        public ActionResult DeleteDesktopOffering(string identifier) {
            ConfigStore.DeleteDesktopOffering(identifier);
            //ConfigStore.Save();
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
