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
        string Description { get; }
        string ZoneId { get; }
        string TemplateId { get; }
        string ServiceOfferingId { get; }
        string NetworkId { get; }
        string HostnamePrefix { get; }
        bool Default { get; }
    }
}
