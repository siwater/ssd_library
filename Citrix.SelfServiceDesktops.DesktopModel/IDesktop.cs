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
    /// Represents a desktop published via the XenDesktop broker and running as a virtual machine in CloudPlatform
    /// </summary>
    public interface IDesktop {

        /// <summary>
        /// Get the Id of the virtual machine (CloudPlatform Id)
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Get the name of the virtual machine (CloudPlatform display name)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gte the primary IP address of the virtual machine
        /// </summary>
        string IpAddress { get; }

        /// <summary>
        /// Get the state of the virtual machine in CloudPlatform
        /// </summary>
        VirtualMachineState State { get; }

        /// <summary>
        /// Get the state of the desktop in XenDesktop
        /// </summary>
        DesktopState DesktopState { get; }

        /// <summary>
        /// Composite state for display
        /// </summary>
        string DisplayState { get; }

    }
}
