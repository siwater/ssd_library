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

namespace Citrix.SelfServiceDesktops.Agent.Services {

    [ServiceContract(Namespace = "urn:com.citrix.selfservicedesktops-15-04-2013")]
    public interface IDesktopService {

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        XElement config(); 
    }
}
