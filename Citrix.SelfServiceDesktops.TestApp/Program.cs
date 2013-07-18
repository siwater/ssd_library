/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp {
    class Program {
        static void Main(string[] args) {

            Console.WriteLine("Tests for Self Service Desktops");
            TestHarness testHarness = new TestHarness();
      
            // Test configuration system is working (you must have SSD windows service running to test the remote retrieval of configXml)
            testHarness.TestConfigurationReader(true);

            //// Test IDesktopManager implementation
            //testHarness.TestDesktopManager();

            // Test SSO capabilities (use of sessionkey and jsessionid to login)
            //testHarness.TestSso();

            // Test Admin Tool capabilities
            Console.WriteLine("Tests for Self Service Desktops Admin");
            TestAdmin.Run();

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}
