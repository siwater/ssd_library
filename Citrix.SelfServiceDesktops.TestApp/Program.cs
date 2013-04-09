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

            TestHarness testHarness = new TestHarness();
            testHarness.TestConfigurationReader();
            testHarness.TestDesktopManager();
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}
