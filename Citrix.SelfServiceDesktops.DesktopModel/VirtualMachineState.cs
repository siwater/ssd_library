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
    /// Current state of the desktop virtual machine (subset of the Apache CloudState virtual machine state)
    /// </summary>
    public  enum VirtualMachineState {
        Unknown,
        Creating,
        Stopped,
        Starting,
        Running,
        Stopping,
        Error,
        Expunging,
        Destroyed
    }

    public static class ValidTransitions
    {
        public static bool CanDelete(VirtualMachineState currState)
        {
            if (currState == VirtualMachineState.Destroyed || currState == VirtualMachineState.Expunging)
            {
                return false;
            }
            return true;
        }
        public static bool CanStart(VirtualMachineState currState)
        {
            if (currState == VirtualMachineState.Stopped)
            {
                return true;
            }
            return false;
        }
        public static bool CanStop(VirtualMachineState currState)
        {
            if (currState == VirtualMachineState.Running ||
                currState == VirtualMachineState.Stopping ||
                currState == VirtualMachineState.Starting)
            {
                return true;
            }
            return false;
        }
        public static bool CanRestart(VirtualMachineState currState)
        {
            if (currState == VirtualMachineState.Running ||
                currState == VirtualMachineState.Starting ||
                currState == VirtualMachineState.Stopping ||
                currState == VirtualMachineState.Starting)
            {
                return true;
            }
            return false;
        }
    }

}
