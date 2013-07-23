using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopModel;


namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {

    [XmlRootAttribute(ElementName = "selfServiceDesktops")]
    public class DesktopServiceConfigurationElement : IDesktopServiceConfiguration {

        #region Internal Classes for Serialization

        public class BrokerElement {
            [XmlAttribute("url")]
            public string Url { get; set; }
        }

        public class AgentElement {
            [XmlAttribute("baseUrl")]
            public string BaseUrl { get; set; }
        }

        public class ListenElement {
            [XmlAttribute("port")]
            public int Port { get; set; }
        }

        public class CloudStackElement {
            [XmlAttribute("url")]
            public string Url { get; set; }

            [XmlAttribute("domain")]
            public string Domain { get; set; }

            [XmlAttribute("hashPassword")]
            public bool HashPassword { get; set; }
        }

        #endregion

        #region Serializable Properties

        [XmlAttribute("remoteConfig")]
        public string RemoteConfig { get; set; }
        
        [XmlElement("cloudstack")]
        public CloudStackElement CloudStack { get; set; }

        [XmlElement("agent")]
        public AgentElement Agent { get; set; }
    
        [XmlElement("broker")]
        public BrokerElement Broker { get; set; }

        [XmlElement("listen")]
        public ListenElement Listen { get; set; }

        [XmlArray("desktopOfferings")]
        [XmlArrayItem(ElementName="add")]
        public List<DesktopOfferingElement> DesktopOfferingsBase { get; set; }

        [XmlElement("script")]
        public PowerShellScriptElement PowerShellScriptBase { get; set; }

        #endregion

        #region IDesktopServiceConfiguration members
     
        /// <summary>
        /// If the base url of the SSD Agent is explicitly specified in config, return that, otherwise
        /// return the base url of the remoteConfig attribute if specified.
        /// </summary>
        /// </summary>
        public Uri AgentUri {
            get {
                Uri result = null;
                if (Agent != null) { 
                    result = ParseUri(Agent.BaseUrl);
                }
                    if ((result == null) && (RemoteConfig != null)) {
                    result =  ParseUri(RemoteConfig);
                    if (result != null) {
                        result = new Uri(result.GetLeftPart(UriPartial.Authority));
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Get the underlying serializable Broker Uri property as a System.Uri. If the underlying
        /// property is null, empty or invalid the getter will return null and log an error.
        /// </summary>
        public Uri BrokerUri {
            get {
                return (Broker != null)  ? ParseUri(Broker.Url) : null;
            }
        }

        /// <summary>
        /// Get the underlying serializable CloudStack Uri property as a System.Uri. If the underlying
        /// property is null, empty or invalid the getter will return null and log an error.
        /// </summary>
        public Uri CloudStackUri {
            get {
                return (CloudStack != null) ? ParseUri(CloudStack.Url) : null;
            }
        }

        public int ListenPort {
            get {
                return (Listen != null) ? Listen.Port : -1;
            }
        }

        public bool HashCloudStackPassword {
            get { 
                return (CloudStack != null) ? CloudStack.HashPassword : false;
            }
        }

        public string Domain {
            get { 
                return (CloudStack != null) ? CloudStack.Domain : null; 
            }
        }

        public IEnumerable<IDesktopOffering> DesktopOfferings {
            get {
                return DesktopOfferingsBase.Select(o => o.MemberwiseClone()).Cast<IDesktopOffering>();                  
            }
        }

        public IPowerShellScript PowerShellScript {
            get {
                return PowerShellScriptBase;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Need to anticipate any old rubbish in the config file. 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private Uri ParseUri(string s) {
            if (!string.IsNullOrEmpty(s)) {
                try {
                    return new Uri(s);
                } catch (Exception e) {
                    CtxTrace.TraceError("Error parsing Url {0} from config: {1}", s, e.Message);
                }
            }
            return null;
        }

        #endregion
    }
}
