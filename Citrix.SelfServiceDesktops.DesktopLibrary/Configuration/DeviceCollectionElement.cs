using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {
   
    public class DeviceCollectionElement : IDeviceCollection {

        #region IDeviceCollection Members

        [XmlAttribute("collection")]
        [Display(Description = "DeviceCollectionNameHelp")]
        public string Name  { get; set; }    

        [XmlAttribute("server")]
        [Display(Description = "DeviceCollectionServerHelp")]
        public string Server  { get; set; }
           

        [XmlAttribute("site")]
        [Display(Description = "DeviceCollectionSiteHelp")]
        public string Site { get; set; }
          

        #endregion
    }
}
