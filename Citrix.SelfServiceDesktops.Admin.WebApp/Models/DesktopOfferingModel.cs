/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;

namespace Citrix.SelfServiceDesktops.Admin.WebApp.Models {

    public class DesktopOfferingModel {

        private const string GuidRegExpr = @"\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}\b";

        private const string ErrorNotGuid = "The field {0} must be a GUID";

        public DesktopOfferingModel() {
        }

        public DesktopOfferingModel(DesktopOfferingElement element) {
            CopyProperties(element, this);
            // This is a PVS desktop offering if any PVS attributes are set
            PvsDesktopOffering = (
                Hypervisor != null ||
                IsoId != null ||
                DiskOfferingId != null ||
                DeviceCollectionElement != null);
        }

        #region Public properties

        [Required]
        [Display(Description = "NameHelp")]
        public string Name { get; set; }

        [Display(Description = "DescriptionHelp")]
        public string Description { get; set; }

        [Required]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        [Display(Description = "ZoneIdHelp")]
        public string ZoneId { get; set; }
     
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        [Display(Description = "TemplateIdHelp")]
        public string TemplateId { get; set; }

        [Display(Description = "IsoIdHelp")]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string IsoId { get; set; }
  
        [Display(Description = "HypervisorHelp")]
        public string Hypervisor { get; set; }

        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        [Display(Description = "DiskOfferingIdHelp")]
        public string DiskOfferingId { get; set; }

        [Required]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        [Display(Description = "ServiceOfferingIdHelp")]
        public string ServiceOfferingId { get; set; }

        [Required]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        [Display(Description = "NetworkIdHelp")]
        public string NetworkId { get; set; }

        [Required]
        [StringLength(11)]
        [Display(Description = "HostnamePrefixHelp")]
        public string HostnamePrefix { get; set; }

        [Display(Name = "Xen Desktop Catalog", Description = "XenDesktopCatalogHelp")]
        public string XenDesktopCatalog { get; set; }

        [Display(Description = "SyncHelp")]
        public bool Sync { get; set; }

        [Display(Name = "PVS Device Collection")]
        public DeviceCollectionElement DeviceCollectionElement { get; set; }

        public bool Default { get; set; }

        public bool PvsDesktopOffering { get; set; }

        #endregion

        public DesktopOfferingElement Update(DesktopOfferingElement element) {
           
            CopyProperties(this, element);
            return element;
        }

        /// <summary>
        /// Memberwise copy of properties from one object to another.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void CopyProperties(object from, object to) {
            Type srcType = from.GetType();
            Type destType = to.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo src in srcType.GetProperties(flags | BindingFlags.GetProperty)) {
                PropertyInfo dest = destType.GetProperty(src.Name, flags | BindingFlags.SetProperty);
                if ((dest != null) && (dest.PropertyType == src.PropertyType)) {
                    dest.SetValue(to, src.GetValue(from));
                }
            }
        }
    }
}