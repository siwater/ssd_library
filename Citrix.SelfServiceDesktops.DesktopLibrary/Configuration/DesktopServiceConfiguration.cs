/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {

    public enum ConfigurationLocation { Local, Remote, Either };
    
    public class DesktopServiceConfiguration : IDesktopServiceConfiguration {


        public static DesktopServiceConfiguration Instance { get { return GetInstance(ConfigurationLocation.Either); } }

        public static DesktopServiceConfiguration GetInstance(ConfigurationLocation location) {
            return new DesktopServiceConfiguration(location);
        }

        private XElement config;

        private DesktopServiceConfiguration(ConfigurationLocation location) {

            string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XDocument doc = XDocument.Load(configFile);
            config = doc.XPathSelectElement("//selfServiceDesktops");           
            if (config == null) {
                throw new ApplicationException("No selfServiceDesktops configuration found in : " + configFile);
            }

            // Load config from remote server
            XAttribute remoteConfigAttribute = config.Attribute("remoteConfig");
            if ((remoteConfigAttribute != null)  && 
                ((location == ConfigurationLocation.Remote) || (location == ConfigurationLocation.Either))) {
                config = GetConfig (new Uri(remoteConfigAttribute.Value));
            }
        }

        private XElement GetConfig(Uri fromUri) {

            HttpWebRequest httpWebRequest = WebRequest.Create(fromUri) as HttpWebRequest;
            httpWebRequest.Method = "GET";
           
            using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse) {
                using (Stream responseStream = httpWebResponse.GetResponseStream()) {
                    using (StreamReader streamReader = new StreamReader(responseStream)) {
                        return XElement.Load(streamReader);
                    }
                }
            }
        }

        #region IDesktopServiceConfiguration members

        public Uri BrokerUri {
            get {
               return new Uri(config.XPathSelectElement("//broker").Attribute("url").Value);
            }
        }

        public Uri CloudStackUri {
            get {
                return new Uri(config.XPathSelectElement("//cloudstack").Attribute("url").Value);
            }
        }

        public int ListenPort {
            get {
                return int.Parse(config.XPathSelectElement("//listen").Attribute("port").Value);
            }
        }

        public bool HashCloudStackPassword {
            get {             
                return bool.Parse(config.XPathSelectElement("//cloudstack").Attribute("hashPassword").Value);
            }
        }

        public string Domain
        {
            get
            {
                return config.XPathSelectElement("//cloudstack").Attribute("domain").Value;
            }
        }

        public IEnumerable<IDesktopOffering> DesktopOfferings {
            get {
                List<IDesktopOffering> result = new List<IDesktopOffering>();
                XmlSerializer deSerializer = new XmlSerializer(typeof(DesktopOfferingElement));
                foreach (XElement e in config.XPathSelectElements("//desktopOfferings/add")) {              
                    DesktopOfferingElement offering = deSerializer.Deserialize(e.CreateReader()) as DesktopOfferingElement;
                    result.Add(offering);
                }
                return result;
            }    
        }

        public IPowerShellScript PowerShellScript {
            get {
                return new PowerShellScriptElement(config);
            }
        }

        #endregion


     
    }
}
