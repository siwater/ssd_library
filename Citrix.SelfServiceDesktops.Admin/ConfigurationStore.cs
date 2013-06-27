using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

using Microsoft.Win32;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.Admin
{
    public class ConfigurationStore : DesktopServiceConfiguration
    {

        private const string ServiceName = "CtxSSDSvc";

        public static ConfigurationStore Open() {
            return Open(GetConfigFilePath(ServiceName));
        }

        public static ConfigurationStore Open(string path) {
            if (!File.Exists(path)) {
                string msg = string.Format("Configuration file [0} does not exist", path);
                throw new ApplicationException(msg);
            }
            XDocument doc = XDocument.Load(path);
            return new ConfigurationStore(doc.XPathSelectElement("//selfServiceDesktops"));
        }

        private ConfigurationStore(XElement config)
            : base(config) {
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
    }
}
