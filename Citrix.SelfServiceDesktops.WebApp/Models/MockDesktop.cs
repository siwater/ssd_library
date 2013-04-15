using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.WebApp
{
    public class MockDesktop : IDesktop
    {
        string name;
        string ipAddress;
        DesktopState state;

        public MockDesktop()
        {
            this.name = "foo";
            this.ipAddress = "255.255.255.255";
            this.state = DesktopState.Stopped;
        }
        public string Name { get { return name; } set { name = value; } }
        public string IpAddress { get { return ipAddress; } set { ipAddress = value; } }
        public DesktopState State { get { return state; } set { state = value; } }

        public string Id
        {
            get { throw new NotImplementedException(); }
        }
    }
}