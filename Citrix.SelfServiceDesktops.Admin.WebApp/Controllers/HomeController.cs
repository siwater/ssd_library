/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Citrix.SelfServiceDesktops.Admin.WebApp.Models;
using Citrix.SelfServiceDesktops.Admin;
using Citrix.SelfServiceDesktops.DesktopModel;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;

namespace Citrix.SelfServiceDesktops.Admin.WebApp.Controllers {

    public class HomeController : Controller {
      
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
                return View(ConfigStore.Configuration.DesktopOfferings);
            } catch (Exception e) {        
                return View("Error", e);
            }
        }

        public ActionResult EditDesktopServiceConfiguration() {
            try {
                DesktopServiceConfigurationElement config = ConfigStore.Configuration as DesktopServiceConfigurationElement;
                return View(new DesktopServiceConfigurationModel(config));
            } catch (Exception e) {
                ViewBag.Message = e.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult EndEditDesktopServiceConfiguration(DesktopServiceConfigurationModel item) {
            if (ModelState.IsValid) {
                item.Update(ConfigStore.EditableConfig);
                ConfigStore.Save();
                return RedirectToAction("Index");
            }
            return View("EditDesktopServiceConfiguration", item);
        }

        [HttpPost]
        public ActionResult NewDesktopOffering() {
            return View("EditDesktopOffering", new DesktopOfferingModel() { Name = "New desktop offering" });
        }

        [HttpPost]
        public ActionResult EditDesktopOffering(string identifier) {         
            DesktopOfferingElement offering = ConfigStore.EditableConfig.DesktopOfferingsBase.Where(o => o.Name == identifier).First();
            return View(new DesktopOfferingModel (offering));
        }

        [HttpPost]
        public ActionResult EndEditDesktopOffering(DesktopOfferingModel item) {
            IDesktopOffering offering = ConfigStore.Configuration.DesktopOfferings.Where(o => o.Name == item.Name).FirstOrDefault();
            if (offering == null) {
                DesktopOfferingElement element = new DesktopOfferingElement();
                ConfigStore.AddDesktopOffering(item.Update(element));
            } else {
                ConfigStore.ReplaceDesktopOffering(item.Update(offering as DesktopOfferingElement));
            }
            ConfigStore.Save();
            return RedirectToAction("ViewDesktopOfferings");
        }
        

        [HttpPost]
        public ActionResult DeleteDesktopOffering(string identifier) {
            ConfigStore.DeleteDesktopOffering(identifier);
            //ConfigStore.Save();
            return RedirectToAction("ViewDesktopOfferings");
        }

        #region Private Methods

        private const string ConfigStoreIndex = "config-store";

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
