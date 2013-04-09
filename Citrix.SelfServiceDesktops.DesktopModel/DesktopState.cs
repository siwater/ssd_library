/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    /// <summary>
    /// Current state of the desktop (subset of the Apache CloudState virtual machine state)
    /// </summary>
    public  enum DesktopState {
        Creating,
        Stopped,
        Starting,
        Running,
        Stopping,
        Error,
        Unknown
    }
}
