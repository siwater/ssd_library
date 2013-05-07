﻿/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Web.Administration;

namespace Citrix.SelfServiceDesktops.Setup.CustomActions {
    /// <summary>
    /// This class contains custom actions for the Wix Installer
    /// </summary>
    public static class CustomActions {

        // This is the Property value of the ComboBox that needs to be populated with a list of web site names
        public const string WebSiteNamesComboBoxId = "WEBSITENAME";

        [CustomAction]
        public static ActionResult GetWebSites(Session session) {
           
            try {
                string query = string.Format("select * from ComboBox where ComboBox.Property='{0}'", WebSiteNamesComboBoxId);
                View view = session.Database.OpenView(query);
                using (ServerManager serverManager = new ServerManager()) {
                    int index = 1;
                    foreach (Site site in serverManager.Sites) {
                        Record record = session.Database.CreateRecord(4);
                        record.SetString(1, "WEBSITENAME");
                        record.SetInteger(2, index++);
                        record.SetString(3, site.Name);
                        record.SetString(4, site.Name);
                        view.Modify(ViewModifyMode.InsertTemporary, record);
                    }
                    return ActionResult.Success;
                }
            } catch (Exception e) {
                session.Log(e.Message);
                session.Log(e.StackTrace);
                return ActionResult.Failure;
            }
        }
    }
}
