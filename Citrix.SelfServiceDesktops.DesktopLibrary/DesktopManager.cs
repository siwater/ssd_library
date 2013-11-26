/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;
using CloudStack.SDK;

namespace Citrix.SelfServiceDesktops.DesktopLibrary {

    using Configuration;

    public class DesktopManager : IDesktopManager {


        public const string DesktopSuffixFormat = "0000";

        // Mulitple instances of the web app may generate name collisions when trying to 
        // create a new desktop. This will tolerate a small number of such collisions byu back off and
        // retry (the DesktopNameSync lock should prevent collisions between sessions in the same web app)
        public const int MaxNameCollisions = 5;
        private static object DesktopNameSync = new object();

        private string DesktopStateUrl;
        private IDesktopServiceConfiguration config;
        private Client cloudStackClient;
        private string userName;

        /// <summary>
        /// Access all areas via port 8096
        /// </summary>
        private Client openAccessClient;

        private DesktopManager(string userName) {
            this.userName = userName;
            config = DesktopServiceConfiguration.Read();
            // Build Uri for accessing open port 8096 as a temporary fix to get complete VM list
            UriBuilder openAccessUriBuilder = new UriBuilder(config.CloudStackUri);
            openAccessUriBuilder.Port = 8096;
            openAccessClient = new Client(openAccessUriBuilder.Uri); 

            Uri agentUri = config.AgentUri;
            if (agentUri != null) {
                DesktopStateUrl = agentUri.ToString() + "desktopstates/";
            } else {
                CtxTrace.TraceWarning("No Agent URI specified: desktop states will be unavailable");
            }
        }

        /// <summary>
        /// Create a new instance of the DesktopManager for the specified user in the default domain
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        internal DesktopManager(string userName, string password)
            : this(userName, password, null) {
        }

        /// <summary>
        /// Create a new instance of the DesktopManager for the specified user and domain
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain">If not specified domain will be taken from configXml value</param>
        internal DesktopManager(string userName, string password, string domain)
            : this(userName) 
        {       
            cloudStackClient = new Client(config.CloudStackUri);
            if (string.IsNullOrEmpty(domain)) {
                domain = config.Domain;
            }
            try {
                cloudStackClient.Login(userName, password, domain, config.HashCloudStackPassword);
            } catch (CloudStackException) {
                cloudStackClient.Login(userName, password, domain, !config.HashCloudStackPassword);
            }
        }

        internal DesktopManager(string userName, string sessionKey, string jSessionId, string domain)
            : this(userName) {
                
            CtxTrace.TraceVerbose("sessionKey={0}, jSessionId={1}", sessionKey, jSessionId);
            Cookie sessionCookie = new Cookie("JSESSIONID", jSessionId);
            sessionCookie.Domain = config.CloudStackUri.Host;
            sessionCookie.Path = "/client";
            cloudStackClient = new Client(config.CloudStackUri, sessionKey, sessionCookie);
        }


        #region IDesktopManager implementation

        public IEnumerable<IDesktopOffering> ListDesktopOfferings() {
            return config.DesktopOfferings.Cast<IDesktopOffering>();
        }

        public IEnumerable<IDesktop> ListDesktops() {
           
            ListVirtualMachinesRequest request = new ListVirtualMachinesRequest();
            ListVirtualMachinesResponse response = cloudStackClient.ListVirtualMachines(request);
            IEnumerable<VirtualMachine> desktopVms = FilterDesktops(response.VirtualMachine, config.DesktopOfferings.Cast<IDesktopOffering>(), false);

            XElement desktopStatus = null;
            if (DesktopStateUrl != null) {
                Uri uri = new Uri(string.Format("{0}{1}", DesktopStateUrl, userName));
                try {
                    desktopStatus = DesktopServiceConfiguration.GetXml(uri);
                } catch (Exception e) {
                    CtxTrace.TraceError("Exception trying to read desktop state from Agent: {0}", e.Message);
                }
            }
            return CreateIDesktopList(desktopVms, desktopStatus);
        } 
  
        /// <summary>
        /// Create a desktop. The boot device may be either an ISO or a conventional template or both.
        /// If the desktop offering specifies both an ISO and a template, the virtual machine is
        /// created from template in the stopped desktopState, and the ISO attached before starting the machine.
        /// Unique name for the new virtual machine is generated by query of CCP. Two levels of protection against name
        /// collisions:
        /// 1) Process wide lock should serialise desktop creation within the same process
        /// 2) Name collision detect with backoff / retry to eliminate collisions with other processes.
        /// </summary>
        /// <param name="desktopOfferingName">desktop offering</param>
        /// <returns>The desktop</returns>
        public IDesktop CreateDesktop(string desktopOfferingName)
        {
            IDesktopOffering offering = ListDesktopOfferings().First(o => (o.Name == desktopOfferingName));

            bool isoAndTemplate = (offering.IsoId != null) && (offering.TemplateId != null);
            string name;

            int nameCollisions = 0;
            while (nameCollisions < MaxNameCollisions) {
                try {
                    lock (DesktopNameSync) {
                        name = GetNextDesktopName(offering);
                        DeployVirtualMachineRequest request = new DeployVirtualMachineRequest() {
                            ServiceOfferingId = offering.ServiceOfferingId,
                            ZoneId = offering.ZoneId,
                            TemplateId = offering.TemplateId,
                            DisplayName = name,
                            // If both ISO and template specified don't start the VM yet
                            StartVm = !isoAndTemplate
                        };
                        request.Parameters["name"] = name;
                        request.WithNetworkIds(offering.NetworkId);
                        if (offering.Hypervisor != null) {
                            request.Parameters["hypervisor"] = offering.Hypervisor;
                        }

                        // If just an ISO is specified boot from that 
                        if (!isoAndTemplate && (offering.IsoId != null)) {
                            request.TemplateId = offering.IsoId;
                        }

                        if (offering.DiskOfferingId != null) {
                            request.Parameters["diskofferingid"] = offering.DiskOfferingId;
                        }

                        string id = cloudStackClient.DeployVirtualMachine(request);

                        if (isoAndTemplate) {
                            APIRequest attachIsoRequest = new APIRequest("attachIso");
                            attachIsoRequest.Parameters["id"] = offering.IsoId;
                            attachIsoRequest.Parameters["virtualmachineid"] = id;
                            XDocument response = cloudStackClient.SendRequest(attachIsoRequest);
                            cloudStackClient.StartVirtualMachine(id);
                        }
                        return new Desktop(id, name, null, VirtualMachineState.Creating, DesktopState.UnknownToXenDesktop);
                    }
                } catch (CloudStackException cse) {
                    if ((cse.APIErrorResult.ErrorCode == "431") &&
                        (cse.APIErrorResult.ErrorText.Contains("already exists"))) {
                        nameCollisions++;
                        CtxTrace.TraceWarning("Name collision creating new desktop (retrying {0} of {1})", nameCollisions, MaxNameCollisions);
                        Random random = new Random();
                        int milliSecondsBackoff = random.Next(1, 10000); // Backoff 1 - 100000 milliseconds.
                        System.Threading.Thread.Sleep(milliSecondsBackoff);
                        continue;
                    }
                    throw cse;
                }
            }
            throw new ApplicationException("Max retries exceeded following name conflicts in CCP");      
        }

        public void DestroyDesktop(string desktopId) {
            APIRequest request = new APIRequest("destroyVirtualMachine");
            request.Parameters["id"] = desktopId;
            cloudStackClient.SendRequest(request);
        }

        public void StartDesktop(string desktopId) {
            APIRequest request = new APIRequest("startVirtualMachine");
            request.Parameters["id"] = desktopId;
            cloudStackClient.SendRequest(request);
        }

        public void StopDesktop(string desktopId) {
            APIRequest request = new APIRequest("stopVirtualMachine");
            request.Parameters["id"] = desktopId;
            cloudStackClient.SendRequest(request);
        }

        public void RestartDesktop(string desktopId) {
            APIRequest request = new APIRequest("rebootVirtualMachine");
            request.Parameters["id"] = desktopId;
            cloudStackClient.SendRequest(request);
        }

        public Uri BrokerUrl {
            get { return config.BrokerUri; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a collection of IDesktop objects from the specified set of virtual machine records and optionally a set of
        /// XenDesktop states.
        /// </summary>
        /// <param name="desktopVms">Collection of VirtualMachines from CloudPlatform</param>
        /// <param name="desktopStates">Optional set of XenDesktop states</param>
        /// <returns>collection of IDesktop objects</returns>
        private IEnumerable<IDesktop> CreateIDesktopList(IEnumerable<VirtualMachine> desktopVms, XElement desktopStates) {
            SortedList<string, IDesktop> result = new SortedList<string, IDesktop>(); 
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            foreach (VirtualMachine vm in desktopVms) {
                DesktopState desktopState = DesktopState.NotProvided;
                if (desktopStates != null) {
                    desktopState = DesktopState.UnknownToXenDesktop;
                    if (!nsmgr.HasNamespace("x")) {
                        XNamespace ns = desktopStates.GetDefaultNamespace();
                        nsmgr.AddNamespace("x", ns.NamespaceName);
                    }
                    string xpath = string.Format("//x:XenDesktopState[x:DesktopName='{0}']/x:State", vm.DisplayName.ToLower());
                    XElement stateElement = desktopStates.XPathSelectElement(xpath, nsmgr);
                    if (stateElement != null) {
                        desktopState = (DesktopState) Enum.Parse(typeof(DesktopState), stateElement.Value);                
                    }
                }
                VirtualMachineState vmstate = Parse(vm.State);
                result.Add(vm.DisplayName, new Desktop(vm.Id, vm.DisplayName, vm.Nic[0].IpAddress, vmstate, desktopState));          
            }
            return result.Values;
        }


        /// <summary>
        /// Filters the list of virtual machines that may be self-service desktops. This will be the set of desktops whose
        /// names match one of the desktop offerings hostname prefixes and has a numeric suffix
        /// </summary>
        /// <param name="machines">The raw set of virtual machines from the CloudStack API</param>
        /// <param name="desktopOfferings">The set of desktop offerings to use as a filter</param>
        /// <param name="includeDestroyed">Include destroyed and expunging virtual machines in the list</param>
        /// <returns>A list of potential desktop virtual machines</returns>
        private IEnumerable<VirtualMachine> FilterDesktops(VirtualMachine[] machines, IEnumerable<IDesktopOffering> desktopOfferings, bool includeDestroyed) {

            List<VirtualMachine> result = new List<VirtualMachine>();
            foreach (VirtualMachine vm in machines) {
                if (desktopOfferings.Count(o => {
                    int num;
                    VirtualMachineState state = Parse(vm.State);
                    return (!string.IsNullOrEmpty(vm.DisplayName) 
                        && vm.DisplayName.StartsWith(o.HostnamePrefix)                         
                        && (includeDestroyed || (state != VirtualMachineState.Expunging && state != VirtualMachineState.Destroyed))                          
                        && int.TryParse(vm.DisplayName.Substring(o.HostnamePrefix.Length), out num));
                }) > 0) {                 
                    result.Add(vm);
                }
            }
            return result;
        }

        private VirtualMachineState Parse(string state) {
            VirtualMachineState result;
            if (!Enum.TryParse(state, true, out result)) {
                CtxTrace.TraceError("Unable to parse desktop state: {0}", state);
                result = VirtualMachineState.Unknown;
            }
            return result;
        }

        /// <summary>
        /// Use the open access client to get all desktops from all users.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<VirtualMachine> ListAllDesktops() {
            ListVirtualMachinesRequest request = new ListVirtualMachinesRequest();
            request.Parameters["listall"] = "true";
            ListVirtualMachinesResponse response = openAccessClient.ListVirtualMachines(request);
            return FilterDesktops(response.VirtualMachine, config.DesktopOfferings.Cast<IDesktopOffering>(), true);
        } 

        /// <summary>
        /// Generate a new desktop name for the specified desktop offering. Non PVS desktops will be named using the host
        /// name prefix plus a free number starting from 1. 
        /// </summary>
        /// <param name="offering">Desktop offering</param>
        /// <returns>A name for the desktop</returns>
        private string GetNextDesktopName(IDesktopOffering offering) {
            string suffix = DesktopSuffixFormat;
            IEnumerable<VirtualMachine> existingDesktops = ListAllDesktops().Where(d => (d.Name.StartsWith(offering.HostnamePrefix)));
            int last = 1;
            if (existingDesktops.Count() > 0) {
                IEnumerable<int> existingSuffixes = existingDesktops.Select(d => {
                    int num = -1;
                    int.TryParse(d.DisplayName.Substring(offering.HostnamePrefix.Length), out num);
                    return num;
                }).Where(i => (i != -1));
                while (existingSuffixes.Contains(last)) {
                    last++;
                }
            }
            suffix = last.ToString(DesktopSuffixFormat);
            return string.Format("{0}{1}", offering.HostnamePrefix, suffix);
        }

        #endregion
    }
}
