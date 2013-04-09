/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citrix.SelfServiceDesktops.DesktopLibrary.Configuration {

    /// <summary>
    /// To give location transparency, the self service desktop configuration is manipulated directly as an Xml fragment. This
    /// placeholder is just to allow the section tp be placed in the app.config file.
    /// </summary>
    public class SelfServiceDesktopsSection : ConfigurationSection {
        public const string SectionName = "selfServiceDesktops"; 
    }
}
