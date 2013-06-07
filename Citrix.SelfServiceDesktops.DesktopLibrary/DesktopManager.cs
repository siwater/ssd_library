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
using System.Xml.Linq;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;
using CloudStack.SDK;

namespace Citrix.SelfServiceDesktops.DesktopLibrary {

    using Configuration;

    public class DesktopManager : IDesktopManager {


        public const string DesktopSuffixFormat = "0000";

        private IDesktopServiceConfiguration config;
        private Client cloudStackClient;

        /// <summary>
        /// Temporary access all areas via port 8096
        /// </summary>
        private Client openAccessClient;

        private DesktopManager() {
            config = DesktopServiceConfiguration.Instance;
            // Build Uri for accessing open port 8096 as a temporary fix to get complete VM list
            UriBuilder openAccessUriBuilder = new UriBuilder(config.CloudStackUri);
            openAccessUriBuilder.Port = 8096;
            openAccessClient = new Client(openAccessUriBuilder.Uri); 
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
        /// <param name="domain">If not specified domain will be taken from config value</param>
        internal DesktopManager(string userName, string password, string domain)
            : this() 
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
            : this() {
                
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
            return ListDesktops(false);
        }

        public IEnumerable<IDesktop> ListDesktops(bool includeDestroyed)
        {
            ListVirtualMachinesRequest request = new ListVirtualMachinesRequest();
            ListVirtualMachinesResponse response = cloudStackClient.ListVirtualMachines(request);
            return FilterDesktops(response.VirtualMachine, config.DesktopOfferings.Cast<IDesktopOffering>(), includeDestroyed);
        }
  
        /// <summary>
        /// Create a desktop. The boot device may be either an ISO or a conventional template or both.
        /// If the desktop offering specifies both an ISO and a template, the virtual machine is
        /// created from template in the stopped state, and the ISO attached before starting the machine.
        /// </summary>
        /// <param name="desktopOfferingName">desktop offering</param>
        /// <returns>The desktop</returns>
        public IDesktop CreateDesktop(string desktopOfferingName)
        {
            IDesktopOffering offering = ListDesktopOfferings().First(o => (o.Name == desktopOfferingName));

            bool isoAndTemplate = (offering.IsoId != null) && (offering.TemplateId != null);
                  
            string name = GetNextDesktopName(offering);
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
            return new Desktop(id, name, null, DesktopState.Creating);
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
        /// Filters the list of virtual machines that may be self-service desktops. This will be the set of desktops whose
        /// names match one of the desktop offerings hostname prefixes and has a numeric suffix
        /// </summary>
        /// <param name="machines">The raw set of virtual machines from the CloudStack API</param>
        /// <param name="desktopOfferings">The set of desktop offerings to use as a filter</param>
        /// <param name="includeDestroyed">Include destroyed and expunging virtual machines in the list</param>
        /// <returns>A list of potential desktops (ordered by name)</returns>
        private IEnumerable<IDesktop> FilterDesktops(VirtualMachine[] machines, IEnumerable<IDesktopOffering> desktopOfferings, bool includeDestroyed) {
           
            SortedList<string, IDesktop> result = new SortedList<string, IDesktop>();
            foreach (VirtualMachine vm in machines) {                
                if (desktopOfferings.Count(o => {
                    int num;
                    DesktopState state = Parse(vm.State);
                    return (vm.DisplayName.StartsWith(o.HostnamePrefix)
                            && (includeDestroyed || (state != DesktopState.Expunging && state != DesktopState.Destroyed))
                            && int.TryParse(vm.DisplayName.Substring(o.HostnamePrefix.Length), out num));
                }) > 0) {
                    DesktopState state = Parse(vm.State);
                    result.Add(vm.Id, new Desktop(vm.Id, vm.DisplayName, vm.Nic[0].IpAddress, state));
                }
            }
            return result.Values;
        }

        private DesktopState Parse(string state) {
            DesktopState result;
            if (!Enum.TryParse(state, true, out result)) {
                CtxTrace.TraceError("Unable to parse desktop state: {0}", state);
                result = DesktopState.Unknown;
            }
            return result;
        }

        /// <summary>
        /// Use the open access client to get all desktops from all users.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IDesktop> ListAllDesktops() {
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
            IEnumerable<IDesktop> existingDesktops = ListAllDesktops().Where(d => (d.Name.StartsWith(offering.HostnamePrefix)));
            int last = 1;
            if (existingDesktops.Count() > 0) {
                IEnumerable<int> existingSuffixes = existingDesktops.Select(d => {
                    int num = -1;
                    int.TryParse(d.Name.Substring(offering.HostnamePrefix.Length), out num);
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
