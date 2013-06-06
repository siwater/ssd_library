/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Citrix.SelfServiceDesktops.DesktopModel
{
    /// <summary>
    /// Asbtract API for provisioning and control of XenDesktop desktops running in CloudStack
    /// </summary>
    public interface IDesktopManager
    {
        IEnumerable<IDesktopOffering> ListDesktopOfferings();
        IEnumerable<IDesktop> ListDesktops();

        IDesktop CreateDesktop(string desktopOfferingId);
        void DestroyDesktop(string desktopId);

        void StartDesktop(string desktopId);
        void StopDesktop(string desktopId);
        void RestartDesktop(string desktopId);

        Uri BrokerUrl { get; }
    }
}
