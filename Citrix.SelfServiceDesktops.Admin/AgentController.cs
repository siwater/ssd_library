using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.Admin {

    public class AgentController {

        private string name;
        private ServiceController controller;
      

        public AgentController(string name) {
            this.name = name;
            this.controller = new ServiceController(name);
        }

        /// <summary>
        /// This method will ensure the service is stopped for the duration of the Action
        /// method. Once the action has completed, the service is restored to its original state
        /// </summary>
        /// <param name="action"></param>
        public void StopFor(Action action) {
            ServiceControllerStatus previousStatus = WaitForStableStatus();
            if (previousStatus != ServiceControllerStatus.Stopped) {
                StopServiceAndWait();        
            }
            action();
            RestoreService(previousStatus);
        }

        private ServiceControllerStatus WaitForStableStatus() {
            while ((controller.Status == ServiceControllerStatus.ContinuePending) ||
                (controller.Status == ServiceControllerStatus.PausePending) ||
                (controller.Status == ServiceControllerStatus.StartPending) ||
                (controller.Status == ServiceControllerStatus.StopPending)) {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                controller.Refresh();
            }
            return controller.Status;
        }

        private void RestoreService(ServiceControllerStatus previousStatus) {
            if (previousStatus == ServiceControllerStatus.Running) {
                controller.Start();
            } else if (previousStatus == ServiceControllerStatus.Paused) {
                controller.Pause();
            }
        }

        private void StopServiceAndWait() {
            controller.Stop();
            while (controller.Status != ServiceControllerStatus.Stopped) {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                controller.Refresh();
            }
        }

    }

}
