using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.MockDesktopLibrary
{
    public class MockDesktopOffering : IDesktopOffering
    {

        string name;
        string description;
        string zoneId;
        string templateId;
        string serviceOfferingId;
        string networkId;
        bool defaultOption = true;

        public MockDesktopOffering()
        {
             name="foo";
             description = "foo";
             zoneId = "foo";
             templateId = "foo";
             serviceOfferingId = "foo";
             networkId = "foo";
        }

        public string Name
        {
            get { return this.name;} set { name = value; }
        }

        public string Description
        {
            get { return this.description;}  set { description = value; }
        }

        public string ZoneId
        {
            get { return this.zoneId;}  set { zoneId = value; }
        }

        public string TemplateId
        {
            get { return this.templateId;}  set { templateId = value; }
        }

        public string ServiceOfferingId
        {
            get { return this.serviceOfferingId;}  set { serviceOfferingId = value; }
        }

        public string NetworkId
        {
            get { return this.networkId;}  set { networkId = value; }
        }

        public bool Default {
            get { return this.defaultOption; }
            set { defaultOption = value; }
        }

        public string HostnamePrefix
        {
            get { return "foobar"; }
        }

        public string IsoId { get { return "foo"; } }

        public string DiskOfferingId { get { return "foo"; } }

        public bool Sync { get { return true; } }

        public string XenDesktopCatalog { get { return "foo"; } }

        public IDeviceCollection DeviceCollection { get { return null; } }
    }
}