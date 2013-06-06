using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {


    //[XmlRootAttribute(ElementName = "device-collection")]
   
    public class DeviceCollectionElement : IDeviceCollection {

        #region IDeviceCollection Members

        [XmlAttribute("collection")]
        public string Name  { get; set; }    

        [XmlAttribute("server")]
        public string Server  { get; set; }
           

        [XmlAttribute("site")]
        public string Site { get; set; }
          

        #endregion
    }
}
