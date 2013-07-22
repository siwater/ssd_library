using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;

namespace Citrix.SelfServiceDesktops.Admin.WebApp.Models {


    public class DesktopServiceConfigurationModel {

        public DesktopServiceConfigurationModel() {
        }

        public DesktopServiceConfigurationModel(DesktopServiceConfigurationElement config) {
            this.AgentUriBase = config.AgentUri == null ? null : config.AgentUri.ToString();
            this.BrokerUri = config.BrokerUri == null ? null : config.BrokerUri.ToString();
            this.CloudStackUri = config.CloudStackUri == null ? null : config.CloudStackUri.ToString();   
            this.Domain = config.Domain;
            this.HashCloudStackPassword = config.HashCloudStackPassword;
            this.ListenPort = config.ListenPort;
            this.PowerShellScript = config.PowerShellScriptBase;
        }

        #region Public properties

        [Url]
        public string AgentUriBase { get; set;  }
        
        [Url]
        public string BrokerUri { get; set;  }
         
        [Url]
        public string CloudStackUri { get; set;  }

        public int ListenPort { get; set;  }

        public bool HashCloudStackPassword { get; set;  }

        public string Domain { get; set;  }

        public PowerShellScriptElement PowerShellScript { get; set; }

        #endregion

        public DesktopServiceConfigurationElement Update(DesktopServiceConfigurationElement config) {
            config.Agent = new DesktopServiceConfigurationElement.AgentElement() { BaseUrl = this.AgentUriBase };
            config.Broker = new DesktopServiceConfigurationElement.BrokerElement() { Url = this.BrokerUri };
            config.CloudStack = new DesktopServiceConfigurationElement.CloudStackElement() {
                Url = this.CloudStackUri,
                Domain = this.Domain,
                HashPassword = this.HashCloudStackPassword
            };
            config.Listen = new DesktopServiceConfigurationElement.ListenElement() { Port = this.ListenPort };
            config.PowerShellScriptBase = PowerShellScript;
            return config;
        }

    }
}