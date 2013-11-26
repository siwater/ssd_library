/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {


    public class DesktopServiceConfiguration {

        public const int NetBiosMaxNameLength = 15;

        #region Public Static Methods

        public static IDesktopServiceConfiguration Read() {
            return ReadFrom(true);
        }

        public static IDesktopServiceConfiguration ReadFrom(bool remoteServer) {
            string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            return new DesktopServiceConfiguration(configFile, remoteServer).Configuration;
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

        #endregion

        protected XDocument configDoc;
        protected string filePath;
        protected DesktopServiceConfigurationElement config;

        protected DesktopServiceConfiguration(string path, bool useRemoteConfig) {
            filePath = path;
            configDoc = XDocument.Load(path);
            XElement configXml = configDoc.XPathSelectElement("//selfServiceDesktops");
            if (configXml == null) {
                throw new ArgumentException("No <selfServiceDesktops/> configuration element found in : " + path);
            }
            config = Deserialize(configXml);

            if (useRemoteConfig && (config.RemoteConfig != null)) {
                try {
                    configXml = GetXml(new Uri(config.RemoteConfig));
                    string remoteConfig = config.RemoteConfig;             
                    config = Deserialize(configXml);
                    // If the remote config does not have an agent URI, infer it from the remote Url in the local config 
                    if (config.AgentUri == null) {
                        config.SetAgentUriFrom(remoteConfig);
                    }
                   
                } catch (Exception e) {
                    throw new ConfigurationErrorsException("Unable to load configuration from remote server", e);
                }
            }        
            ValidateConfiguration();
        }

        #region Public Properties

        public IDesktopServiceConfiguration Configuration { get { return config; } }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sanity check the supplied configuration
        /// </summary>
        /// <param name="configXml"></param>
        protected void ValidateConfiguration() {

            try {
                try {
                    // Check desktop offering names are unique
                    Configuration.DesktopOfferings.ToDictionary(i => i.Name);
                } catch (ArgumentException e) {
                    throw new ConfigurationErrorsException("Desktop Offerings must have unique name", e);
                }
                try {
                    // Check desktop offering host name prefixes are unique
                    Configuration.DesktopOfferings.ToDictionary(i => i.HostnamePrefix);
                } catch (ArgumentException e) {
                    throw new ConfigurationErrorsException("Desktop Offerings must specify unique host name prefixes", e);
                }

                // Check the configuration will generate legal Windows computer names (NetBios name <= 15 characters)
                int maxPrefixLen = NetBiosMaxNameLength - DesktopManager.DesktopSuffixFormat.Length;
                foreach (IDesktopOffering offering in Configuration.DesktopOfferings) {
                    if (offering.HostnamePrefix.Length > maxPrefixLen) {
                        string msg = string.Format("HostNamePrefix for desktop offering {0} exceeds {1} characters", offering.Name, maxPrefixLen);
                        throw new ConfigurationErrorsException(msg);
                    }
                }

                // Check sanity of device collections
                foreach (IDesktopOffering offering in Configuration.DesktopOfferings) {
                    IDeviceCollection dc = offering.DeviceCollection;
                    if (dc != null) {
                        if (string.IsNullOrEmpty(dc.Name) || string.IsNullOrEmpty(dc.Server) || string.IsNullOrEmpty(dc.Site)) {
                            string msg = string.Format("Illegal DeviceCollection: Name ({0}), Server ({1}) Site ({2})", dc.Name, dc.Server, dc.Site);
                            throw new ConfigurationErrorsException(msg);
                        }
                    }
                }

                // Ensure there is exactly one default desktop offering
                IEnumerable<IDesktopOffering> defaultSet = Configuration.DesktopOfferings.Where(i => i.Default == true);
                if (defaultSet.Count() > 1) {
                    throw new ConfigurationErrorsException("Only one Desktop Offerings may marked with default=true");
                }
                if (defaultSet.Count() == 0) {
                    DesktopOfferingElement first = Configuration.DesktopOfferings.First() as DesktopOfferingElement;
                    first.Default = true;
                }
            } catch (ConfigurationErrorsException ex) {
                CtxTrace.TraceError(ex);
                throw;
            }
        }

        /// <summary>
        /// Serialize the configuration to Xml
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        protected XElement Serialize(DesktopServiceConfigurationElement config) {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(DesktopServiceConfigurationElement));
                MemoryStream stream = new MemoryStream();
                serializer.Serialize(stream, config);
                stream.Position = 0;
                return XElement.Load(stream);
            } catch (Exception e) {
                CtxTrace.TraceError(e);
                throw;
            }
        }

        protected DesktopServiceConfigurationElement Deserialize(XElement configXml) {
            try {
                XmlSerializer deSerializer = new XmlSerializer(typeof(DesktopServiceConfigurationElement));
                return deSerializer.Deserialize(configXml.CreateReader()) as DesktopServiceConfigurationElement;
            } catch (Exception e) {
                CtxTrace.TraceError(e);
                throw;
            }
        }

        #endregion

    }
}
