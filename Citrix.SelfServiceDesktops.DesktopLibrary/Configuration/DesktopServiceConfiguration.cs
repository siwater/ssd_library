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

        public const int NetBiosMaxNameLength = 15;

        public static DesktopServiceConfiguration Instance { get { return GetInstance(ConfigurationLocation.Either); } }

        public static DesktopServiceConfiguration GetInstance(ConfigurationLocation location) {
            string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XDocument doc = XDocument.Load(configFile);
            XElement config = doc.XPathSelectElement("//selfServiceDesktops");
            if (config == null) {
                throw new ConfigurationErrorsException("No <selfServiceDesktops/> configuration element found in : " + configFile);
            }

            // Load config from agent 
            XAttribute remoteConfigAttribute = config.Attribute("remoteConfig");
            if (remoteConfigAttribute != null) {
                string remoteUrl = remoteConfigAttribute.Value;
                Uri uri = new Uri(remoteUrl);
                if ((location == ConfigurationLocation.Remote) || (location == ConfigurationLocation.Either)) {
                    config = GetXml(new Uri(remoteUrl));
                }
            }
            return new DesktopServiceConfiguration(config);
        }

        public static XElement GetXml(Uri fromUri) {

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

        private XElement config;

        public DesktopServiceConfiguration(XElement config) {
            this.config = config;
            XAttribute remoteConfigAttribute = config.Attribute("remoteConfig");
            if (remoteConfigAttribute != null) {
                string remoteUrl = remoteConfigAttribute.Value;
                Uri uri = new Uri(remoteUrl);
                AgentUri = new Uri(uri.GetLeftPart(UriPartial.Authority));
            }
            ValidateConfiguration();
        }

        #region IDesktopServiceConfiguration members

        public Uri AgentUri {
            get;
            private set;
        }

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

        #region Private Methods

        /// <summary>
        /// Sanity check the supplied configuration
        /// </summary>
        /// <param name="config"></param>
        private void ValidateConfiguration() {      
            try {
                // Check desktop offering names are unique
                this.DesktopOfferings.ToDictionary(i => i.Name);
            } catch (ArgumentException e) {
                throw new ConfigurationErrorsException("Desktop Offerings must have unique name", e);
            }
            try {
                // Check desktop offering host name prefixes are unique
                this.DesktopOfferings.ToDictionary(i => i.HostnamePrefix);
            } catch (ArgumentException e) {
                throw new ConfigurationErrorsException("Desktop Offerings must specify unique host name prefixes", e);
            }

            // Check the configuration will generate legal Windows computer names (NetBios name <= 15 characters)
            int maxPrefixLen = NetBiosMaxNameLength - DesktopManager.DesktopSuffixFormat.Length;
            foreach (IDesktopOffering offering in DesktopOfferings) {
                if (offering.HostnamePrefix.Length > maxPrefixLen) {
                    string msg = string.Format("HostNamePrefix for desktop offering {0} exceeds {1} characters", offering.Name, maxPrefixLen);
                    throw new ConfigurationErrorsException(msg);
                }
            }
        }

        #endregion

    }
}
