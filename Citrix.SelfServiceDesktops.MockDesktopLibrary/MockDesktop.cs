using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.MockDesktopLibrary
{
    public class MockDesktop : IDesktop
    {
        string name;
        string ipAddress;
        VirtualMachineState state;
        DesktopState desktopState;

        public MockDesktop()
        {
            this.name = "foo";
            this.ipAddress = "255.255.255.255";
            this.state = VirtualMachineState.Stopped;
            this.desktopState = DesktopState.Unknown;
        }
        public string Name { get { return name; } set { name = value; } }
        public string IpAddress { get { return ipAddress; } set { ipAddress = value; } }
        public VirtualMachineState State { get { return state; } set { state = value; } }
        public DesktopState DesktopState { get { return desktopState; } set { desktopState = value; } }
        public string DisplayState { get { return State.ToString(); } }

        public string Id
        {
            get { return "1"; }
        }

    }
}