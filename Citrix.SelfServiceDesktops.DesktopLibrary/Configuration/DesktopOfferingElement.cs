/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {

    [XmlRootAttribute(ElementName = "add")]
    public class DesktopOfferingElement : IDesktopOffering {

        public DesktopOfferingElement() {
            Sync = true;
        }

        [Required]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [Required]
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

        [Required]
        [XmlAttribute("service-offering-id")]
        public string ServiceOfferingId { get; set; }
         
        [Required]
        
        [XmlAttribute("network-id")]
        public string NetworkId { get; set; }

        [Required]
        [StringLength(11)]
        [XmlAttribute("hostname-prefix")]
        public string HostnamePrefix { get; set; }

        [XmlAttribute("xendesktop-catalog")]
        public string XenDesktopCatalog { get; set; }

        [XmlAttribute("sync")]
        public bool Sync { get; set; }

        [XmlElement("device-collection")]
        public DeviceCollectionElement DeviceCollectionImpl { get; set; }

        [XmlIgnore]
        public IDeviceCollection DeviceCollection { get { return DeviceCollectionImpl; } }

        public override string ToString() {
            return string.Format("{0} ({1})", Name, Description);
        }

        public bool Default {
            get { return false; }
        }
    }
}
