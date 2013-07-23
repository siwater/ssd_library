/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {

    public class DesktopOfferingElement : IDesktopOffering {

        public DesktopOfferingElement() {
            Sync = true;
        }

        #region Serializable Attributes

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("zone-id")]
        public string ZoneId { get; set; }

        [XmlAttribute("template-id")]
        public string TemplateId { get; set; }

        [XmlAttribute("iso-id")]
        public string IsoId { get; set; }

        [XmlAttribute("hypervisor")]
        public string Hypervisor { get; set; }

        [XmlAttribute("disk-offering-id")]
        public string DiskOfferingId { get; set; }
   
        [XmlAttribute("service-offering-id")]
        public string ServiceOfferingId { get; set; }
         
        [XmlAttribute("network-id")]
        public string NetworkId { get; set; }

        [XmlAttribute("hostname-prefix")]
        public string HostnamePrefix { get; set; }

        [XmlAttribute("xendesktop-catalog")]
        public string XenDesktopCatalog { get; set; }

        [XmlAttribute("sync")]
        public bool Sync { get; set; }

        [XmlElement("device-collection")]
        public DeviceCollectionElement DeviceCollectionElement { get; set; } 
          
        [XmlAttribute("default")]
        public bool Default { get; set; }

        #endregion

        [XmlIgnore]
        public IDeviceCollection DeviceCollection { get { return DeviceCollectionElement; } }

        public override string ToString() {
            return string.Format("{0} ({1})", Name, Description);
        }

        public new DesktopOfferingElement MemberwiseClone() {
            return base.MemberwiseClone() as DesktopOfferingElement;
        }
        
    }
}
