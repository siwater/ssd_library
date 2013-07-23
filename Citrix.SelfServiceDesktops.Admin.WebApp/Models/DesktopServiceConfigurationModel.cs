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
            this.AgentUriBase = config.Agent == null ? null : config.Agent.BaseUrl;
            this.BrokerUri = config.Broker == null ? null : config.Broker.Url;
            this.CloudStackUri = config.CloudStack == null ? null : config.CloudStack.Url;   
            this.Domain = config.Domain;
            this.HashCloudStackPassword = config.HashCloudStackPassword;
            this.ListenPort = config.ListenPort;
            this.ScriptPath = config.PowerShellScriptBase.Path;
            this.Frequency = config.PowerShellScriptBase.FrequencyBase;
            this.DebugScript = config.PowerShellScriptBase.Debug;
        }

        #region Public properties

        [ValidUri]
        public string AgentUriBase { get; set;  }

        [ValidUri]
        public string BrokerUri { get; set;  }

        [ValidUri]
        public string CloudStackUri { get; set;  }

        public int ListenPort { get; set;  }

        public bool HashCloudStackPassword { get; set;  }

        public string Domain { get; set;  }

        public string ScriptPath { get; set; }

        [TimeSpan]
        public string Frequency { get; set; }

        public bool DebugScript { get; set; }

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
            config.PowerShellScriptBase = new PowerShellScriptElement() { 
                Path = ScriptPath,
                FrequencyBase = Frequency,
                Debug = DebugScript
            };
            return config;
        }

        #region Custom Validation

        /// <summary>
        /// Using a custom validation attribute as the standard UrlAttribute insists on fully qualified hostname,
        /// and regards http://localhost/ et al. as invalid (!). Note that a null Url is acceptable
        /// </summary>
        [AttributeUsage(AttributeTargets.Property |  AttributeTargets.Field, AllowMultiple = false)]
        public class ValidUriAttribute : ValidationAttribute {

            public override bool IsValid(object value) {
                try {
                    string url = value as string;
                    if (!string.IsNullOrEmpty(url)) {
                        Uri uri = new Uri(value as string);
                    }
                    return true;
                } catch {
                    return false;
                }
            }

            public override string FormatErrorMessage(string name) {
                return string.Format("The {0} property must be a valid Url", name);
            }
        }

        /// <summary>
        /// Check the supplied field or property is a valid TimeSpan
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
        public class TimeSpanAttribute : ValidationAttribute {

            public override bool IsValid(object value) {
                TimeSpan result;
                return TimeSpan.TryParse(value as string, out result);
            }

            public override string FormatErrorMessage(string name) {
                return string.Format("The {0} property must be a valid TimeSpan (e.g. 00:00:30)", name);
            }
        }

        #endregion

    }
}