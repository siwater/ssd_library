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

        public XElement Script { get; private set; }

        internal PowerShellScriptElement(XElement config) {
            Script = config.XPathSelectElement("//script");
        }

        public string Path {
            get { 
                if (Script != null) {
                    XAttribute path = Script.Attribute("path");
                    if (path != null) {
                        return path.Value;
                    }
                }
                return null;
            }
        }

        public TimeSpan? Frequency {
            get {
                if (Script != null) {
                    return TimeSpan.Parse(Script.Attribute("frequency").Value);
                }
                return null;
            }     
        }

        public bool Debug {
            get {             
                return (Script != null) ? bool.Parse(Script.Attribute("debug").Value) : false;             
            }
        }
    }
}
