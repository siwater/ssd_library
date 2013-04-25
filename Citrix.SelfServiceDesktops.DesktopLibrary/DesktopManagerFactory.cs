/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary {

    public class DesktopManagerFactory : IDesktopManagerFactory {

        public IDesktopManager CreateManager(string userName, string password) {
            return new DesktopManager(userName, password);     
        }

        public IDesktopManager CreateManager(string userName, string password, string domain) {
            return new DesktopManager(userName, password, domain);
        }

        public IDesktopManager CreateManager(string userName, string sessionKey, string jSessionId, string domain)
        {
            throw new NotImplementedException();
        }
    }
}
