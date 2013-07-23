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
        }

        #region Public properties

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string ZoneId { get; set; }

     
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string TemplateId { get; set; }

    
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string IsoId { get; set; }

      
        public string Hypervisor { get; set; }

        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string DiskOfferingId { get; set; }

        [Required]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string ServiceOfferingId { get; set; }

        [Required]
        [RegularExpression(GuidRegExpr, ErrorMessage = ErrorNotGuid)]
        public string NetworkId { get; set; }

        [Required]
        [StringLength(11)]
        public string HostnamePrefix { get; set; }
  
        public string XenDesktopCatalog { get; set; }
   
        public bool Sync { get; set; }

        public DeviceCollectionElement DeviceCollectionElement { get; set; }

        public bool Default { get; set; }

        #endregion

        public DesktopOfferingElement Update(DesktopOfferingElement element) {
           
            CopyProperties(this, element);
            return element;
        }

        /// <summary>
        /// Memberiwse copy of properties from one object to another.
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