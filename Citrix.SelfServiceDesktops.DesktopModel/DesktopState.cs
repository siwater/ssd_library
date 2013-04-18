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
        Unknown,
        Creating,
        Stopped,
        Starting,
        Running,
        Stopping,
        Error,
        Destroyed
    }

    public static class ValidTransitions
    {
        public static bool CanDelete(DesktopState currState)
        {
            if (currState == DesktopState.Destroyed)
            {
                return false;
            }
            return true;
        }
        public static bool CanStart(DesktopState currState)
        {
            if (currState == DesktopState.Stopped)
            {
                return true;
            }
            return false;
        }
        public static bool CanStop(DesktopState currState)
        {
            if (currState == DesktopState.Running ||
                currState == DesktopState.Stopping ||
                currState == DesktopState.Starting)
            {
                return true;
            }
            return false;
        }
        public static bool CanRestart(DesktopState currState)
        {
            if (currState == DesktopState.Running ||
                currState == DesktopState.Starting ||
                currState == DesktopState.Stopping ||
                currState == DesktopState.Starting)
            {
                return true;
            }
            return false;
        }
    }

}
