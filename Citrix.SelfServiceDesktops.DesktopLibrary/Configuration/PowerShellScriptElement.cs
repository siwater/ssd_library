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

        #region Serializable Attributes

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("frequency")]
        public string FrequencyBase { get; set; }

        [XmlAttribute("debug")]
        public bool Debug { get; set; }

        #endregion

        [XmlIgnore]
        public TimeSpan? Frequency {
            get {
                if (string.IsNullOrEmpty(FrequencyBase)) {
                    return null;
                }
                return TimeSpan.Parse(FrequencyBase);            
            }
            set {
                FrequencyBase = value.HasValue ? value.ToString() : null;
            }
        }
    }
}
