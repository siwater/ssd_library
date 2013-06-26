using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citrix.SelfServiceDesktops.Admin;
using Citrix.SelfServiceDesktops.DesktopModel;
namespace TestApp {
    class TestAdmin {

        public static void Run() {

            ConfigurationStore store = ConfigurationStore.Open();
            Console.WriteLine("Got configuration store");

            foreach (IDesktopOffering offering in store.DesktopOfferings) {
                Console.WriteLine("Offering {0}", offering.Name);
            }

        }
    }
}
