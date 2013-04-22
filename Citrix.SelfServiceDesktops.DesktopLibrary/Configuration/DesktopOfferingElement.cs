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

    [XmlRootAttribute(ElementName = "add")]
    public class DesktopOfferingElement : IDesktopOffering {

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("zone-id")]
        public string ZoneId { get; set; }

        [XmlAttribute("template-id")]
        public string TemplateId { get; set; }

        [XmlAttribute("service-offering-id")]
        public string ServiceOfferingId { get; set; }

        [XmlAttribute("network-id")]
        public string NetworkId { get; set; }

        [XmlAttribute("hostname-prefix")]
        public string HostnamePrefix { get; set; }


        public override string ToString() {
            return string.Format("{0} ({1})", Name, Description);
        }
    }
}
