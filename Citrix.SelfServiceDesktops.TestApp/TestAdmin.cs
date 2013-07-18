using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

using Citrix.SelfServiceDesktops.Admin;
using Citrix.SelfServiceDesktops.DesktopModel;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;

namespace TestApp {
    class TestAdmin {

        public static void TestNewLoad() {
            string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XDocument configDoc = XDocument.Load(configFile);
            XElement config = configDoc.XPathSelectElement("//selfServiceDesktops");
            if (config != null) {
                try {
                    XmlSerializer deSerializer = new XmlSerializer(typeof(DesktopServiceConfigurationElement));
                    DesktopServiceConfigurationElement el = deSerializer.Deserialize(config.CreateReader()) as DesktopServiceConfigurationElement;
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }

        }

        public static void Run() {

            ConfigurationStore store = ConfigurationStore.Open();
            Console.WriteLine("Got configuration store");
            TestHarness.DisplayOfferings(store.Configuration.DesktopOfferings);
            TestAddOffering(store);
            TestAddDuplicateOffering(store);
            TestReplaceOffering(store);
            TestReplaceOfferingNoMatch(store);
            TestDeleteOffering(store);
            TestDeleteOfferingNoMatch(store);

            // Test writing the store
            store = ConfigurationStore.Open();
            Console.WriteLine("Reloaded new configuration store");
            TestReplaceOffering(store);
            store.EditableConfig.Broker.Url = "http://somewhere/XenDesktop"; 
            store.Save();
        }

        public static void TestAddOffering(ConfigurationStore store) {
            //DesktopOfferingElement offering = new DesktopOfferingElement() ;
            DesktopOfferingElement offering = store.Configuration.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Name = "new offering";
            Console.WriteLine("Add new Desktop Offering {0}", offering.Name);
            store.AddDesktopOffering(offering);
            IDesktopOffering result = store.Configuration.DesktopOfferings.FirstOrDefault(o => o.Name == offering.Name);
            Console.WriteLine((result != null) ? "PASS: offering was added" : "FAIL: offering  not added");  
        }

        public static void TestAddDuplicateOffering(ConfigurationStore store) {
            IDesktopOffering existing = store.Configuration.DesktopOfferings.First();
            Console.WriteLine("Add duplicate Desktop Offering {0}", existing.Name);
            DesktopOfferingElement offering = new DesktopOfferingElement() {
                Name = existing.Name,
            };
            try {
                store.AddDesktopOffering(offering);
                Console.WriteLine("FAIL: added a duplicate desktop offering to the store");
            } catch (ArgumentException e) {
                Console.WriteLine("PASS: Exception thrown: {0}", e.Message);
            }
        }

        public static void TestReplaceOffering(ConfigurationStore store) {
           
            DesktopOfferingElement offering =  store.Configuration.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Description += " (modified)";             
            Console.WriteLine("Modify Desktop Offering {0}", offering.Name);
            store.ReplaceDesktopOffering(offering);
            IDesktopOffering result = store.Configuration.DesktopOfferings.FirstOrDefault(o => o.Description == offering.Description);
            Console.WriteLine((result != null) ? "PASS: offering was modified" : "FAIL: offering  not modified");             
        }

        public static void TestReplaceOfferingNoMatch(ConfigurationStore store) {
            DesktopOfferingElement offering = new DesktopOfferingElement() {
                Name = "should not match",
                Description = "this has been modified"
            };
            Console.WriteLine("Modify Desktop Offering {0}", offering.Name);
            try {
                store.ReplaceDesktopOffering(offering);
                Console.WriteLine("FAIL: replaced with a new name");
            } catch (ArgumentException e) {
                Console.WriteLine("PASS: Exception thrown: {0}", e.Message);
            }
        }

        public static void TestDeleteOffering(ConfigurationStore store) {
            IDesktopOffering existing = store.Configuration.DesktopOfferings.First();
            Console.WriteLine("Delete Desktop Offering {0}", existing.Name);
            store.DeleteDesktopOffering(existing.Name);
            IDesktopOffering result = store.Configuration.DesktopOfferings.FirstOrDefault(o => o.Name == existing.Name);
            Console.WriteLine( (result == null) ? "PASS: offering was deleted" : "FAIL: offering still exists");         
        }

        public static void TestDeleteOfferingNoMatch(ConfigurationStore store) {     
            string Name = "should not match";    
            Console.WriteLine("Delete Desktop Offering {0}", Name);
            try {
                store.DeleteDesktopOffering(Name);
                Console.WriteLine("FAIL: replaced with a new name");
            } catch (ArgumentException e) {
                Console.WriteLine("PASS: Exception thrown: {0}", e.Message);
            }
        }
    }
}
