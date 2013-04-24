/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Citrix.SelfServiceDesktops.Agent.Services {

    [ServiceBehavior(Name = "ConfigService",
                     Namespace = "urn:com.citrix.selfservicedesktops.config-15-04-2013",
                     IncludeExceptionDetailInFaults = true)]
    public class DesktopService : IDesktopService {


        public XElement config() {
            return getConfig();
        }

        public static XElement getConfig() {
            XDocument doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            return doc.XPathSelectElement("//selfServiceDesktops");
        }
    }
}
