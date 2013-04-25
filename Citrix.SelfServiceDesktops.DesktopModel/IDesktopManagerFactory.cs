/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    public interface IDesktopManagerFactory {
        IDesktopManager CreateManager(string userName, string password);

        IDesktopManager CreateManager(string userName, string password, string domain);

        IDesktopManager CreateManager(string userName, string sessionKey, string jSessionId, string domain);
    }
}
