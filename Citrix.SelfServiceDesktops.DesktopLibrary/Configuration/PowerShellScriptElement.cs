using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {
    
    public class PowerShellScriptElement : IPowerShellScript {

         private XElement config;

         internal PowerShellScriptElement(XElement config) {
             this.config = config;
         }

        public string Path {
            get { 
                XElement script = config.XPathSelectElement("//script");
                if (script != null) {
                    XAttribute path = script.Attribute("path");
                    if (path != null) {
                        return path.Value;
                    }
                }
                return null;
            }
        }

        public TimeSpan Frequency {
            get { 
                return TimeSpan.Parse(config.XPathSelectElement("//script").Attribute("frequency").Value); 
            }
        }

        public bool Debug {
            get {
                try {
                    return bool.Parse(config.XPathSelectElement("//script").Attribute("debug").Value);
                } catch (NullReferenceException) {
                    return false;
                }
            }
        }
    }
}
