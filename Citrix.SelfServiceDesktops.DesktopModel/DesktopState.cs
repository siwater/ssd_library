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

        // The desktop state has not be provided (e.g. if the Agent is not available)
        NotProvided,

        // Before the machine is added to XenDesktop via the Add-BrokerMachine command, it is unknown to XenDesktop
        UnknownToXenDesktop,

        // The machine is known to XenDesktop but the VDA is not (yet) registered
        Unregistered,

        // The machine is known to XenDesktop and available for use
        Available,

        // The machine is known to XenDesktop and there is an active HDX session to the desktop
        InUse,

        //  The machine is known to XenDesktop and there is a disconnected HDX session to the desktop
        Disconnected,

        // The machine is known to XenDesktop but the state has not been recognised
        Error

    }
}
