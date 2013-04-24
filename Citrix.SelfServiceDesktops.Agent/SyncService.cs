using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Citrix.Diagnostics;

using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.Agent {
    
    public class SyncService {

        private IDesktopServiceConfiguration config;
        private string scriptPath;
        private CancellationTokenSource cancellationTokenSource;
        private Task syncTask;

        public void Start() {
            CtxTrace.TraceInformation();
            config = DesktopServiceConfiguration.Instance;
            scriptPath = GetScriptPath(config.PowerShellScript.Path);
            cancellationTokenSource = new CancellationTokenSource();
            syncTask = Task.Factory.StartNew(() => SyncDesktopOfferings(), cancellationTokenSource.Token);
        }

        public void Stop() {
            CtxTrace.TraceInformation();
            cancellationTokenSource.Cancel();     
        }

        #region Private Methods

        private string GetScriptPath (string configScriptPath) {
            if (File.Exists(configScriptPath)) {
       
                if (!Path.IsPathRooted(configScriptPath)) {
                    return ".\\" + configScriptPath;
                }
                return configScriptPath;
            }

            string scriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configScriptPath);
            if (File.Exists(scriptPath)) {
                return scriptPath;
            }
            throw new System.Configuration.ConfigurationErrorsException("Unable to locate script " + configScriptPath);
        }


        private void SyncDesktopOfferings() {
            try {
                CtxTrace.TraceInformation();
                TimeSpan syncFrequency = config.PowerShellScript.Frequency;

                while (!cancellationTokenSource.Token.IsCancellationRequested) {
                    foreach (DesktopOfferingElement offering in config.DesktopOfferings) {
                        SyncDesktopOfferings(offering);
                    }
                    // Ensure timely response to task cancel
                    DateTime waitUntil = DateTime.Now + syncFrequency;
                    while (!cancellationTokenSource.Token.IsCancellationRequested && (DateTime.Now < waitUntil)) {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    }
                }
                CtxTrace.TraceVerbose("Task is cancelled");
            } catch (Exception e) {
                CtxTrace.TraceError(e);
            }
        }

        private void SyncDesktopOfferings(DesktopOfferingElement offering) {
            CtxTrace.TraceInformation(offering.Name);
            Dictionary<string, string> args = new Dictionary<string, string>();
            args["ccpip"] = config.CloudStackUri.Host;
            args["hostnameprefix"] = offering.HostnamePrefix;
          
            try {
                PsWrapper script = new PsWrapper(scriptPath, config.PowerShellScript.Debug);
                script.IgnoreExceptions.Add("ADIdentityNotFoundException");
                Collection<PSObject> results = script.RunPowerShell(args);
                foreach (PSObject result in results) {
                    CtxTrace.TraceInformation(result);
                }
            } catch (Exception e) {
                CtxTrace.TraceError(e);
            }
        }

        #endregion
    }
}
