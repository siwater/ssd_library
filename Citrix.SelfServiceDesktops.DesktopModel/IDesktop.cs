/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    /// <summary>
    /// Represents a desktop published via the XenDesktop broker and running as a virtual machine in Apache CloudStack
    /// </summary>
    public interface IDesktop {
        string Id { get; }
        string Name { get; }
        string IpAddress { get; }
        DesktopState State { get; }

    }
}
