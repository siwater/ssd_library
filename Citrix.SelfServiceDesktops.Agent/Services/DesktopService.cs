/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;


namespace Citrix.SelfServiceDesktops.Agent.Services {

    [ServiceBehavior(Name = "DesktopService", Namespace = "urn:com.citrix.selfservicedesktops-12-06-2013")]
    public class DesktopService : IDesktopService {

        public const string scriptName = "ListDesktops.ps1";

        public XElement getConfig() {
            XDocument doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            return doc.XPathSelectElement("//selfServiceDesktops");
        }

        public List<XenDesktopState> getDesktopStates(string username) {
            CtxTrace.TraceVerbose("Get desktop state for user {0}", username);
            List<XenDesktopState> result = new List<XenDesktopState>();
            try {
                PsWrapper script = new PsWrapper(scriptName);
                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments["user"] = username;
                Collection<PSObject> psResults = script.RunPowerShell(arguments);
                foreach (PSObject obj in psResults) {
                    // The script should return a Hash object for each desktop 
                    Hashtable desktop = obj.BaseObject as Hashtable;                
                    if (desktop != null) {
                        string machineName = (string)desktop["machine-name"];
                        DesktopState state = ParseDesktopState((string)desktop["summary-state"]);
                        result.Add(new XenDesktopState() { DesktopName = machineName, State = state });
                    } else {
                        CtxTrace.TraceError("Unexpected return object from {0}: {1}", scriptName, obj.BaseObject);
                    }
                }
            } catch (Exception e) {
                CtxTrace.TraceError(e);
            }
            return result;
        }

        private DesktopState ParseDesktopState(string desktopState) {
            DesktopState result = DesktopState.Error;
            if (!Enum.TryParse(desktopState, out result)) {
                CtxTrace.TraceError("Unrecognised XenDesktop state {0}", desktopState);
            }
            return result;
        }
    }
}
