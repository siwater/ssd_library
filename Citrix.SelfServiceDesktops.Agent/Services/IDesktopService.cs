/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Citrix.SelfServiceDesktops.Agent.Services {


    [ServiceContract(Namespace = "urn:com.citrix.selfservicedesktops-12-06-2013")]
    public interface IDesktopService {

        [OperationContract]
        [WebGet(UriTemplate = "config", ResponseFormat = WebMessageFormat.Xml)]
        XElement getConfig();

        [OperationContract]
        [WebGet(UriTemplate = "desktopstates/{username}", ResponseFormat = WebMessageFormat.Xml)]
        List<XenDesktopState> getDesktopStates(string username);
    }
}
