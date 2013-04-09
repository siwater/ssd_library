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

        public Desktop(string id, string name, string ipAddress, DesktopState state) {
            Id = id;
            Name = name;
            IpAddress = ipAddress;
            State = state;
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

        public DesktopState State {
            get;
            private set;
        }

        public override string ToString() {
            return string.Format("Desktop: {0}; State: {2}; IpAddress: {3}", Name, Id, State, IpAddress);
        }
    }
}
