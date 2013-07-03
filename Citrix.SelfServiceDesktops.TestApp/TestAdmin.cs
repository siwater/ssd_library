using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.Admin;
using Citrix.SelfServiceDesktops.DesktopModel;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;

namespace TestApp {
    class TestAdmin {

        public static void Run() {

            ConfigurationStore store = ConfigurationStore.Open();
            Console.WriteLine("Got configuration store");
            Show(store.DesktopOfferings);
            TestAdd(store);
            TestAddDuplicate(store);
            TestReplace(store);
            TestReplaceNoMatch(store);
            TestDelete(store);
            TestDeleteNoMatch(store);

            // Test writing the store
            store = ConfigurationStore.Open();
            Console.WriteLine("Reloaded new configuration store");
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Description = string.Format("Modified {0}", DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
            store.ReplaceDesktopOffering(offering);
            store.Save();
        }

        public static void TestAdd(ConfigurationStore store) {
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Name = "new offering";
            Console.WriteLine("Add new Desktop Offering {0}", offering.Name);
            store.AddDesktopOffering(offering);
            IDesktopOffering result = store.DesktopOfferings.Where(o => o.Name == offering.Name).FirstOrDefault();
            Console.WriteLine((result != null) ? "PASS: offering was added" : "FAIL: offering  not added");  
        }

        public static void TestAddDuplicate(ConfigurationStore store) {
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            Console.WriteLine("Add duplicate Desktop Offering {0}", offering.Name);
            try {
                store.AddDesktopOffering(offering);
                Console.WriteLine("FAIL: added a duplicate desktop offering to the store");
            } catch (ArgumentException e) {
                Console.WriteLine("PASS: Exception thrown: {0}", e.Message);
            }
        }

        public static void TestReplace(ConfigurationStore store) {
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Description = "this has been modified";
            Console.WriteLine("Modify Desktop Offering {0}", offering.Name);
            store.ReplaceDesktopOffering(offering);
            IDesktopOffering result = store.DesktopOfferings.Where(o => o.Description == offering.Description).FirstOrDefault();
            Console.WriteLine((result != null) ? "PASS: offering was modified" : "FAIL: offering  not modified");             
        }

        public static void TestReplaceNoMatch(ConfigurationStore store) {
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Name = "should not match";
            offering.Description = "this has been modified";
            Console.WriteLine("Modify Desktop Offering {0}", offering.Name);
            try {
                store.ReplaceDesktopOffering(offering);
                Console.WriteLine("FAIL: replaced with a new name");
            } catch (ArgumentException e) {
                Console.WriteLine("PASS: Exception thrown: {0}", e.Message);
            }
        }

        public static void TestDelete(ConfigurationStore store) {
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            Console.WriteLine("Delete Desktop Offering {0}", offering.Name);
            store.DeleteDesktopOffering(offering.Name);
            IDesktopOffering result = store.DesktopOfferings.Where(o => o.Name == offering.Name).FirstOrDefault();
            Console.WriteLine( (result == null) ? "PASS: offering was deleted" : "FAIL: offering still exists");         
        }

        public static void TestDeleteNoMatch(ConfigurationStore store) {
            DesktopOfferingElement offering = store.DesktopOfferings.First() as DesktopOfferingElement;
            offering.Name = "should not match";    
            Console.WriteLine("Delete Desktop Offering {0}", offering.Name);
            try {
                store.DeleteDesktopOffering(offering.Name);
                Console.WriteLine("FAIL: replaced with a new name");
            } catch (ArgumentException e) {
                Console.WriteLine("PASS: Exception thrown: {0}", e.Message);
            }
        }

        private static void Show(IEnumerable<IDesktopOffering> offerings) {
            Console.WriteLine("Desktop offerings list: ");
            foreach (IDesktopOffering offering in offerings) {
                Console.WriteLine("{0} [{1}]", offering.Name, offering.Description);
            }
        }
    }
}
