using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.Agent;

namespace Citrix.SelfServiceDesktops.Agent.Host {

    /// <summary>
    /// Convenience wrapper to run the Self Service Desktops Agent as a console app 
    /// (note: needs to be run as a XenDesktop Administrator)
    /// </summary>
    class Program {
        static void Main(string[] args) {

            CtxTrace.Initialize("self-service-desktops-agent-host", true);
            DesktopAgentService svc = new DesktopAgentService();
            svc.Start();
            Console.WriteLine("Desktop Agent is running");
            Console.WriteLine("Press any key to stop");
            Console.ReadLine();
            svc.Stop();
            Console.WriteLine("Desktop Agent is stopped");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
