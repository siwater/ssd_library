/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;
using Citrix.SelfServiceDesktops.DesktopModel;
using Citrix.Diagnostics;

namespace Citrix.SelfServiceDesktops.Agent {

    using Services;

    /// <summary>
    /// This is the container for the various services run inside the Self Service Desktops Agent, It can be executed in a range 
    /// of contexts (console app, windows service etc)
    /// </summary>
    public class DesktopAgentService {

        private const string ListenUrlPattern = "http://localhost:{0}/";

        private Listener listener;
        private SyncService syncService; 

        public void Start() {
            CtxTrace.TraceInformation();
            StartWebListener();
            syncService = new SyncService();
            syncService.Start();
        }

        private void StartWebListener() {
            CtxTrace.TraceInformation();
            IDesktopServiceConfiguration config = DesktopServiceConfiguration.Read();

            Uri listenUrl = new Uri(String.Format(ListenUrlPattern, config.ListenPort));
            WebServiceHost host = new WebServiceHost(typeof(DesktopService), listenUrl);
            host.AddServiceEndpoint(typeof(IDesktopService), new WebHttpBinding(), "");
            ServiceDebugBehavior sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            sdb.HttpHelpPageEnabled = false;
            listener = new Listener(host);
            listener.Start();
        }

        public void Stop() {
            CtxTrace.TraceInformation();
            listener.Stop();
            syncService.Stop();
        }
    }
}
