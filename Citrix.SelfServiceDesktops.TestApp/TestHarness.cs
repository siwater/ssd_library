/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopLibrary;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;
using Citrix.SelfServiceDesktops.DesktopModel;

namespace TestApp {
    public class TestHarness {

        private List<string> operationsInProgress;

        public TestHarness() {
            CtxTrace.Initialize("self-service-desktops-agent", true);
        }

        public void TestPs1() {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments["catalogname"] = "who cares";
            arguments["ccpip"] = "192.168.1.1";
            arguments["domain"] = "formentera";
            arguments["templatestart"] = "DEV";
            PsRunner.TestRunScript(@".\Test.ps1", arguments);
        }

        public void TestConfigurationReader() {
            IDesktopServiceConfiguration config = DesktopServiceConfiguration.Instance;
            Console.WriteLine("Broker Url is {0}", config.BrokerUri);
            Console.WriteLine("CloudStack Url is {0}", config.CLoudStackUri);
            DisplayOfferings(config.DesktopOfferings);

            //config = DesktopServiceConfiguration.GetInstance(ConfigurationLocation.Remote);
            //Console.WriteLine("Broker Url is {0}", config.BrokerUri);
            //Console.WriteLine("CloudStack Url is {0}", config.CLoudStackUri);
            //DisplayOfferings(config.DesktopOfferings);

        }


        public void TestDesktopManager() {
            DesktopManagerFactory factory = new DesktopManagerFactory();
            IDesktopManager manager = factory.CreateManager("admin", "1pass@word1");

            IEnumerable<IDesktopOffering> offerings = manager.ListDesktopOfferings();
            DisplayOfferings(offerings);

            IEnumerable<IDesktop> desktops = manager.ListDesktops();
            DisplayDesktops(desktops);
            int count = desktops.Count();

            IDesktop newDesktop = manager.CreateDesktop(offerings.First().Name);
            Console.WriteLine("New desktop is {0}", newDesktop);

            // Check the new desktop appears in the list
            desktops = manager.ListDesktops();
            if (desktops.Count() != count + 1) {
                Console.WriteLine("Error: Got {0} desktops in list; expected {1}", desktops.Count(), count + 1);
                throw new ApplicationException("DesktopManager fail");
            }

            operationsInProgress = new List<string>();

            // Try delete operation on a desktop
            desktops = manager.ListDesktops();
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == DesktopState.Running) {
                    Console.WriteLine("Attempting to destroy {0}", d);
                    manager.DestroyDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }
              
            // Try start operation on a desktop     
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == DesktopState.Stopped) {
                    Console.WriteLine("Attempting start of {0}", d);
                    manager.StartDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            // Try stop operation on a desktop
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == DesktopState.Running) {
                    Console.WriteLine("Attempting stop of {0}", d);
                    manager.StopDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            // Try restart operation on a desktop
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == DesktopState.Running) {
                    Console.WriteLine("Attempting restart of {0}", d);
                    manager.RestartDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            desktops = manager.ListDesktops();
            DisplayDesktops(desktops);  
        }

        private bool operationInPogress(string id) {
            return operationsInProgress.Contains(id);
        }

        private void DisplayDesktops(IEnumerable<IDesktop> desktops) {
            Console.WriteLine("There are {0} desktops", desktops.Count());
            foreach (IDesktop desktop in desktops) {
                Console.WriteLine("Got desktop : {0}", desktop);
            }
        }


        private void DisplayOfferings(IEnumerable<IDesktopOffering> offerings) {
            foreach (IDesktopOffering offering in offerings) {
                Console.WriteLine("Desktop offering: {0}", offering);
            }
        }

        
    }
}
