using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

using Microsoft.Win32;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.Admin
{

    /// <summary>
    /// This class extends the base read-only class with methods to update the configuration. It operates on the configuration
    /// file of the Agent installed on the local machine. 
    /// </summary>
    public class ConfigurationStore : DesktopServiceConfiguration
    {
        private const string ServiceName = "CtxSSDSvc";

        public static ConfigurationStore Open() {
            return Open(GetConfigFilePath(ServiceName));
        }

        public static ConfigurationStore Open(string path) {
            if (!File.Exists(path)) {
                string msg = string.Format("Configuration file {0} does not exist", path);
                throw new System.IO.FileNotFoundException(msg);
            }
            if (!FileUtilities.IsWriteable(path)) {
                string msg = string.Format("You do not have write access to Configuration file {0}", path);
                throw new UnauthorizedAccessException(msg);
            }
            return new ConfigurationStore(path);
        }

        private ConfigurationStore(string path)
            : base(path, false) {
        }

        public DesktopServiceConfigurationElement EditableConfig {
            get {
                return config;
            }
        }

        public void Save() {
            AgentController controller = new AgentController(ServiceName);
            controller.StopFor(() => this.WriteFile());
        }
        
        public void AddDesktopOffering(DesktopOfferingElement offering) {
            DesktopOfferingElement existing = GetDesktopOffering(offering.Name);
            if (existing != null) {
                throw new ArgumentException("Duplicate desktop offering name");
            }
            config.DesktopOfferingsBase.Add(offering);
        }

        public void DeleteDesktopOffering(string name) {
            DesktopOfferingElement existing = GetDesktopOffering(name);
            if (existing == null) {
                throw new ArgumentException("Desktop offering does not exist");
            }
            config.DesktopOfferingsBase.Remove(existing);
        }

        public void ReplaceDesktopOffering(DesktopOfferingElement offering) {
            DeleteDesktopOffering(offering.Name);
            AddDesktopOffering(offering);
        }

        #region Private Methods

        private DesktopOfferingElement GetDesktopOffering(string name) {
            return config.DesktopOfferingsBase.FirstOrDefault(i => (i.Name == name));
        }

        private static string GetConfigFilePath(string serviceName) {
            string keyName = string.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);
            if (key != null) {           
                string exe = (string) key.GetValue("ImagePath");
                if (exe != null) {
                    return exe.Trim('\"') + ".config";
                }
            }
            return null;
        }

        private void WriteFile() {     
           XElement newConfig = Serialize(config);
           configDoc.XPathSelectElement("//selfServiceDesktops").ReplaceWith(newConfig);        
           configDoc.Save(this.filePath);
        }

   

        #endregion
    }
}
