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

        public void Save() {
            AgentController controller = new AgentController(ServiceName);
            controller.StopFor(() => this.WriteFile());
        }
        
        public void AddDesktopOffering(IDesktopOffering offering) {
            XElement existing = GetDesktopOffering(offering.Name);
            if (existing != null) {
                throw new ArgumentException("Duplicate desktop offering name");
            }
            config.XPathSelectElement("//desktopOfferings").Add(Serialize(offering));
        }

        public void ReplaceDesktopOffering(IDesktopOffering offering) {
            XElement existing = GetDesktopOffering(offering.Name);
            if (existing == null) {
                throw new ArgumentException("Desktop offering does not exist");
            }
            existing.ReplaceWith(Serialize(offering));
        }

        public void DeleteDesktopOffering(string name) {
            XElement existing = GetDesktopOffering(name);
            if (existing == null) {
                throw new ArgumentException("Desktop offering does not exist");
            }
            existing.Remove();
        }

        #region Private Methods

        private XElement GetDesktopOffering(string name) {
            string xpath = string.Format("//desktopOfferings/add[@name='{0}']", name);
            return config.XPathSelectElement(xpath);
        }

        private static XElement Serialize(IDesktopOffering offering) {
            XmlSerializer serializer = new XmlSerializer(offering.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, offering);
            stream.Position = 0;
            return XElement.Load(stream);
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
            configDoc.Save(this.filePath);
        }

        #endregion
    }
}
