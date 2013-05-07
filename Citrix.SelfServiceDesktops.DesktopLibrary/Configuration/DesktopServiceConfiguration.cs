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

        private const string configServiceUrlPattern = "http://localhost:{0}/config";

        private const int defaultAgentPort = 8000;

        private static string _configServiceUrl;

        public static string ConfigServiceUrl {
            get {
                if (_configServiceUrl == null) {
                    _configServiceUrl = GetConfigServiceUrl();
                }
                return _configServiceUrl;
            }
        }

        public static DesktopServiceConfiguration Instance { get { return GetInstance(ConfigurationLocation.Either); } }

        public static DesktopServiceConfiguration GetInstance(ConfigurationLocation location) {
            return new DesktopServiceConfiguration(location);
        }

        private static string GetConfigServiceUrl() {
            ushort port = defaultAgentPort;
            string agentPort = ConfigurationManager.AppSettings["agent-port"];
            if (!string.IsNullOrEmpty(agentPort)) {
                ushort.TryParse(agentPort, out port);
            }        
            return string.Format(configServiceUrlPattern, port);
        }

        private XElement config;

        private DesktopServiceConfiguration(ConfigurationLocation location) {

            // Try to read config from local app.config.
            if ((location == ConfigurationLocation.Local) || (location == ConfigurationLocation.Either)) {
                XDocument doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                config = doc.XPathSelectElement("//selfServiceDesktops");
            }

            // Try to read the config from remote server
            if ((config == null) && (location != ConfigurationLocation.Local)) {
                config = GetConfig(new Uri(ConfigServiceUrl));
            }

            if (config == null) {
                throw new ApplicationException("No configuration found at location: " + location);
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
