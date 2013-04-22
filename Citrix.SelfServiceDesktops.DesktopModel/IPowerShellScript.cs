using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Citrix.SelfServiceDesktops.DesktopModel {

    public interface IPowerShellScript {

        /// <summary>
        /// Relative or absolute path for the powershell script
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Frequency at which the script should be run
        /// </summary>
        TimeSpan Frequency { get; }

        /// <summary>
        /// Whether to pass a "debug" swtich to the script
        /// </summary>
        bool Debug { get; }
    }
}
