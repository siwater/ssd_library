/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary {

    public class Desktop : IDesktop {

        public Desktop(string id, string name, string ipAddress, VirtualMachineState state, DesktopState desktopState) {
            Id = id;
            Name = name;
            IpAddress = ipAddress;
            State = state;
            DesktopState = desktopState;
        }

        public string Id {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }

        public string IpAddress {
            get;
            private set;
        }

        public VirtualMachineState State {
            get;
            private set;
        }

        public DesktopState DesktopState {
            get;
            private set;
        }

        /// <summary>
        /// The state for display on the GUI is normally the Virtual Machine State from CloudPlatform.
        /// However whilst the deskop is being prepared and registered with XenDesktop the state
        /// "Preparing" is displayed.
        /// </summary>
        public string DisplayState {
            get {
                if ((State == VirtualMachineState.Running) &&
                    ((DesktopState == DesktopModel.DesktopState.UnknownToXenDesktop) ||
                    (DesktopState == DesktopModel.DesktopState.Unregistered))) {
                    return "Preparing";
                }
                return State.ToString();
            }
        }

        public override string ToString() {
            return string.Format("Desktop: {0}; State: {2}; IpAddress: {3}", Name, Id, State, IpAddress);
        }
    }
}
