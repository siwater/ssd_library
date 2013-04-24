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

using Citrix.Diagnostics;

namespace Citrix.SelfServiceDesktops.Agent {

    using Services;

    /// <summary>
    /// This is the container for the various services run inside the Self Service Desktops Agen, It can be executed in a range 
    /// of contexts (console app, windows service etc)
    /// </summary>
    public class DesktopAgentService {

        public const string ListenUrl = "http://localhost:8000/";

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
            Uri uri = new Uri(ListenUrl);
            WebServiceHost host = new WebServiceHost(typeof(DesktopService), uri);
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
