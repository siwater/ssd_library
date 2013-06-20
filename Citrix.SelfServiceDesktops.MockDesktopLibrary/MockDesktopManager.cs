using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.MockDesktopLibrary
{
    public class MockDesktopManager : IDesktopManager
    {
        Dictionary<string, IDesktop> desktops = new Dictionary<string, IDesktop>();
        Dictionary<string, IDesktopOffering> offerings = new Dictionary<string, IDesktopOffering>();

        public MockDesktopManager()
        {
            MockDesktop d1 = new MockDesktop() { Name = "foo1", IpAddress = "192.168.0.1", State = VirtualMachineState.Stopped };
            MockDesktop d2 = new MockDesktop() { Name = "foo2", IpAddress = "192.168.0.2", State = VirtualMachineState.Stopped };
            MockDesktop d3 = new MockDesktop() { Name = "foo3", IpAddress = "192.168.0.3", State = VirtualMachineState.Stopped };
            desktops.Add(d1.Name, d1);
            desktops.Add(d2.Name, d2);
            desktops.Add(d3.Name, d3);

            MockDesktopOffering o1 = new MockDesktopOffering()
                {
                    Name = "Office",
                    Description = "Office Tools with a short description",
                    NetworkId = "OfficeVlan",
                    ServiceOfferingId = "OfficeResources",
                    TemplateId = "OfficeImage",
                    ZoneId = "1"
                };
            MockDesktopOffering o2 = new MockDesktopOffering()
            {
                Name = "Developer",
                Description = "Developer Tools with a longer description to test if the web pages lays this out correctly",
                NetworkId = "DevVlan",
                ServiceOfferingId = "DevResources",
                TemplateId = "DevImage",
                ZoneId = "1"
            };
            MockDesktopOffering o3 = new MockDesktopOffering()
            {
                Name = "Isolated",
                Description = "Tools, but locked down security wise",
                NetworkId = "PublicVlan",
                ServiceOfferingId = "OfficeResources",
                TemplateId = "SecureImage",
                ZoneId = "1"
            };
            MockDesktopOffering o4 = new MockDesktopOffering() {
                Name = "PVS with server cache",
                Description = "Streamed desktop from PVS with server side cache",
                NetworkId = "PublicVlan",
                ServiceOfferingId = "OfficeResources",
                TemplateId = "SecureImage",
                ZoneId = "1"
            };
            MockDesktopOffering o5 = new MockDesktopOffering() {
                Name = "PVS with client cache",
                Description = "Streamed desktop from PVS with client side cache to minimize network bandwidth consumption",
                NetworkId = "PublicVlan",
                ServiceOfferingId = "OfficeResources",
                TemplateId = "SecureImage",
                ZoneId = "1"
            };

            offerings.Add(o1.Name, o1);
            offerings.Add(o2.Name, o2);
            offerings.Add(o3.Name, o3);
            offerings.Add(o4.Name, o4);
            offerings.Add(o5.Name, o5);
        }

        public IEnumerable<IDesktopOffering> ListDesktopOfferings()
        {
            return offerings.Values;
        }

        public IEnumerable<IDesktop> ListDesktops()
        {
            return desktops.Values;
        }

        public IDesktop CreateDesktop(string serviceOfferingId)
        {
            MockDesktop d1 = new MockDesktop() { 
                Name = Guid.NewGuid().ToString(),
                IpAddress = "192.168.0.1", State = VirtualMachineState.Stopped 
            };
            desktops.Add(d1.Name, d1);

            return d1;
        }

        public void DestroyDesktop(string desktop)
        {
            desktops.Remove(desktop);
        }

        public void StartDesktop(string desktop)
        {
            MockDesktop desk = desktops[desktop] as MockDesktop; 
            desk.State = VirtualMachineState.Running;
        }

        public void StopDesktop(string desktop)
        {
            MockDesktop desk = desktops[desktop] as MockDesktop;
            desk.State = VirtualMachineState.Stopped;
        }

        public void RestartDesktop(string desktop)
        {
            MockDesktop desk = desktops[desktop] as MockDesktop;
            desk.State = VirtualMachineState.Running;
        }

        public Uri BrokerUrl
        {
            get { return new Uri("https://go.citrix.com/vpn/index.html"); }
        }
    }
}