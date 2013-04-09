﻿/*
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

        Uri CLoudStackUri { get; }

        IEnumerable<IDesktopOffering> DesktopOfferings { get; }

    }
}
