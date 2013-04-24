/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Citrix.Diagnostics;

namespace Citrix.SelfServiceDesktops.Agent {

    public partial class AgentService : ServiceBase {

        public AgentService() {
            InitializeComponent();
        }

        public static string DisplayName {
            get {
                return "Citrix SelfService Desktop Agent";
            }
        }

        public static string Name {
            get {
                return "CtxDsAgnt";
            }
        }

        public static string Description {
            get {
                return "Maintain sync between desktop hosting platform (CloudStack) and desktop access broker (XenDesktop)";
            }
        }

        public static void StopService() {
            ServiceController ctl = new ServiceController(Name);
            ctl.Stop();
        }

        private DesktopAgentService _Service;

        protected override void OnStart(string[] args) {
            CtxTrace.TraceInformation();
            _Service = new DesktopAgentService();
            _Service.Start();
           
        }

        protected override void OnStop() {
            CtxTrace.TraceInformation();
            if (_Service != null) {
                _Service.Stop();
            }
        }
    }
}
