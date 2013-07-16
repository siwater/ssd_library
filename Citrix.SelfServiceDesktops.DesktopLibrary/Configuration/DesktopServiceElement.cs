using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.DesktopModel;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {

    public class DesktopServiceElement : IDesktopServiceConfiguration {

        #region IDesktopServiceConfiguration members

        [Url]
        public Uri AgentUri { get; set; }      

        [Url]
        public Uri BrokerUri  { get; set; }          

        [Url]
        public Uri CloudStackUri { get; set; }

        public int ListenPort { get; set; }

        public bool HashCloudStackPassword { get; set; }

        public string Domain  { get; set; }

        public IEnumerable<IDesktopOffering> DesktopOfferings  { get; set; }
       
        public IPowerShellScript PowerShellScript  { get; set; }

        #endregion
    }
}
