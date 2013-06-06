/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    public interface IDesktopServiceConfiguration {

        Uri BrokerUri { get; }

        Uri CloudStackUri { get; }

        int ListenPort { get; }

        bool HashCloudStackPassword { get; }

        string Domain { get; }

        IEnumerable<IDesktopOffering> DesktopOfferings { get; }

        IPowerShellScript PowerShellScript { get; }

    }
}
