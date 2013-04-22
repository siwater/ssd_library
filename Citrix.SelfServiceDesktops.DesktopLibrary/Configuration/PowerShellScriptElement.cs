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
                return config.XPathSelectElement("//script").Attribute("path").Value; 
            }
        }

        public TimeSpan Frequency {
            get { 
                return TimeSpan.Parse(config.XPathSelectElement("//script").Attribute("frequency").Value); 
            }
        }

        public bool Debug {
            get { 
                return bool.Parse(config.XPathSelectElement("//script").Attribute("debug").Value); 
            }
        }
    }
}
