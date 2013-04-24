/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.ServiceModel;
using System.Threading;

using Citrix.Diagnostics;

namespace Citrix.SelfServiceDesktops.Agent.Services {
    
    /// <summary>
    /// Wrapper class to support multiple ServiceHost instances (each running in its own thread)
    /// </summary>
    public class Listener {

        public ServiceHost Host { get; private set; }

        private AutoResetEvent _StopEvent;

        public Listener (ServiceHost host) {
            Host = host;
            _StopEvent = new AutoResetEvent (false);
        }

        public void Start () {
            ThreadPool.QueueUserWorkItem (new WaitCallback (WorkerThread));
        }

        public void Stop () {
            _StopEvent.Set ();
        }

        private void WorkerThread (object Data) {
            CtxTrace.TraceVerbose ();        
            string url = Host.BaseAddresses[0].ToString ();
            try {
                Host.Open ();
                CtxTrace.TraceInformation ("Listening on " + url);
                _StopEvent.WaitOne ();
                CtxTrace.TraceInformation ("Stop listening on " + url);
                Host.Close ();
            } catch (Exception e) {
                CtxTrace.TraceError (e);
            }
            CtxTrace.TraceInformation ("Thread Exiting");
        }
    }
}
