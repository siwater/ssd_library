/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    /// <summary>
    /// Current state of the desktop (this reflects the XenDesktop state - see Get-BrokerDesktop PowerShell command)
    /// </summary>
    public enum DesktopState {

        // Before the machine is added to XenDesktop via the Add-BrokerMachine command, it it not known to XenDesktop
        Unknown,

        // The machine has been added to XenDesktop bu the VDA is not (yet) registered
        Unregistered,

        // The machine is available for use
        Available,

        // There is an active HDX session to the desktop
        InUse,

        // The machine has a disconnected HDX remote session
        Disconnected,

        // The machine is know to XenDesktop but is in an unknown or error state
        Error

    }
}
