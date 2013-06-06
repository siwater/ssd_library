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
    
    /// <summary>
    /// This is the service that periodically executes a PowerShell script to synchronise XenDesktop and CloudPlatform
    /// The script will be executed for each Desktop Offering in the config.
    /// </summary>
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
                        if (offering.Sync) {
                            SyncDesktopOfferings(offering);
                        }
                    }
                    // Ensure timely response to task cancel
                    DateTime waitUntil = DateTime.Now + syncFrequency;
                    while (!cancellationTokenSource.Token.IsCancellationRequested && (DateTime.Now < waitUntil)) {
                        Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                    }
                }
                CtxTrace.TraceVerbose("Task is cancelled");
            } catch (Exception e) {
                CtxTrace.TraceError(e);
            }
        }

        private void SyncDesktopOfferings(DesktopOfferingElement offering) {
            CtxTrace.TraceVerbose("{0}, {1}", offering.Name, offering.HostnamePrefix);
            
            // Arguments to pass to PowerShell script.
            Dictionary<string, object> args = new Dictionary<string, object>();
 
            args["ccpip"] = config.CloudStackUri.Host;
            args["hostnameprefix"] = offering.HostnamePrefix;

            if (!string.IsNullOrEmpty(offering.XenDesktopCatalog)) {
                args["catalogname"] = offering.XenDesktopCatalog;
            }
            if (!string.IsNullOrEmpty(offering.TemplateId)) {
                args["templateid"] = offering.TemplateId;
            }
            if (!string.IsNullOrEmpty(offering.IsoId)) {
                args["isoid"] = offering.IsoId;
            }
            args["devicecollection"] = offering.DeviceCollection;
          
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
