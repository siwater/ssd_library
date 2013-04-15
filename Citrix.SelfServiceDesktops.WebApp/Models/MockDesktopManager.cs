using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.WebApp
{
    public class MockDesktopManager : IDesktopManager
    {
        Dictionary<string, IDesktop> desktops = new Dictionary<string, IDesktop>();
        Dictionary<string, IDesktopOffering> offerings = new Dictionary<string, IDesktopOffering>();

        public MockDesktopManager()
        {
            MockDesktop d1 = new MockDesktop() { Name = "foo1", IpAddress = "192.168.0.1", State = DesktopState.Stopped };
            MockDesktop d2 = new MockDesktop() { Name = "foo2", IpAddress = "192.168.0.2", State = DesktopState.Stopped };
            MockDesktop d3 = new MockDesktop() { Name = "foo3", IpAddress = "192.168.0.3", State = DesktopState.Stopped };
            desktops.Add(d1.Name, d1);
            desktops.Add(d2.Name, d2);
            desktops.Add(d3.Name, d3);

            MockDesktopOffering o1 = new MockDesktopOffering()
                {
                    Name = "Office",
                    Description = "Office Tools",
                    NetworkId = "OfficeVlan",
                    ServiceOfferingId = "OfficeResources",
                    TemplateId = "OfficeImage",
                    ZoneId = "1"
                };
            MockDesktopOffering o2 = new MockDesktopOffering()
            {
                Name = "Developer",
                Description = "Developer Tools",
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

            offerings.Add(o1.Name, o1);
            offerings.Add(o2.Name, o2);
            offerings.Add(o3.Name, o3);
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
                IpAddress = "192.168.0.1", State = DesktopState.Stopped 
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
            desk.State = DesktopState.Running;
        }

        public void StopDesktop(string desktop)
        {
            MockDesktop desk = desktops[desktop] as MockDesktop;
            desk.State = DesktopState.Stopped;
        }

        public void RestartDesktop(string desktop)
        {
            MockDesktop desk = desktops[desktop] as MockDesktop;
            desk.State = DesktopState.Running;
        }

        public Uri BrokerUrl
        {
            get { return new Uri("https://go.citrix.com/vpn/index.html"); }
        }
    }
}