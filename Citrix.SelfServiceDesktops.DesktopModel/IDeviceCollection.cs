using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Citrix.SelfServiceDesktops.DesktopModel {


    /// <summary>
    /// Represents a PVS device collection
    /// </summary>
    public interface IDeviceCollection {

        /// <summary>
        /// Name of the device collection
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Name of the PVS Site
        /// </summary>
        string Site { get; }

        /// <summary>
        /// Name or IP address of the PVS server
        /// </summary>
        string Server { get; }
    }
}
