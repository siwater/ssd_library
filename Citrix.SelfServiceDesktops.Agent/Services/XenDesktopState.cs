/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.Agent.Services {
    
    public class XenDesktopState {

        public string DesktopName { get; set; }

        public DesktopState State { get; set; }
    }
}
