using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        public Uri AgentUri {
            get {
                if ((Agent != null) && (Agent.BaseUrl != null)) {
                    return new Uri(Agent.BaseUrl);
                } else if (RemoteConfig != null) {
                    Uri remoteUri = new Uri(RemoteConfig);
                    return new Uri(remoteUri.GetLeftPart(UriPartial.Authority));
                }
                return null;
            }
        }

        public Uri BrokerUri {
            get {
                return ((Broker != null) && (Broker.Url != null)) ? new Uri(Broker.Url) : null;
            }
        }

        public Uri CloudStackUri {
            get {
                return ((CloudStack != null) && (CloudStack.Url != null)) ? new Uri(CloudStack.Url) : null;
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
    }
}
