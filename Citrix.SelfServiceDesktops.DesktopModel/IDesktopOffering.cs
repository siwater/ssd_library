/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    public interface IDesktopOffering {
        /// <summary>
        /// The name must be unique
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional decscription of the Desktop Offering
        /// </summary>
        string Description { get; }

        /// <summary>
        /// CloudPlatform zone in which desktops should be deployed
        /// </summary>
        string ZoneId { get; }

        /// <summary>
        /// CloudPlatform template to use for desktops
        /// </summary>
        string TemplateId { get; }

        /// <summary>
        /// Optional ISO id to be attached to the desktop on boot
        /// </summary>
        string IsoId { get; }

        /// <summary>
        /// CLoudPlatform service offering id
        /// </summary>
        string ServiceOfferingId { get; }

        /// <summary>
        /// CloudPlatform zone in which desktops should be deployed
        /// </summary>
        string NetworkId { get; }

        /// <summary>
        /// Name prefix to be used for naming desktops (a numberic suffix will be appended)
        /// </summary>
        string HostnamePrefix { get; }

        /// <summary>
        /// Indicate whether this is the default desktop offering
        /// </summary>
        bool Default { get; }

        /// <summary>
        /// Whether instances created from this desktop offering should be synchronised 
        /// between CloudPlatform and XenDesktop
        /// </summary>
        bool Sync { get; }

        /// <summary>
        /// The catalog name to use in XenDesktop to register desktops of this type
        /// </summary>
        string XenDesktopCatalog { get; }

        /// <summary>
        /// Optional PVS device collection (streamed desktops)
        /// </summary>
        IDeviceCollection DeviceCollection { get; }
    
    }
}
