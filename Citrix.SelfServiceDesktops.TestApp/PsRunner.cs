/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Xml;

namespace TestApp {
    class PsRunner {


        public static void TestRunScript(string scriptPath, Dictionary<string, string> arguments) {
            Collection<PSObject> results = RunScript(scriptPath, arguments);
            foreach (PSObject result in results) {
                
                Console.Write("Result type : " + result.BaseObject.GetType().FullName + "; ");
                Console.WriteLine(result.BaseObject);

                XmlDocument doc = result.BaseObject as XmlDocument;
                if (doc != null) {
                    Console.WriteLine(doc.InnerXml);
                }       
            }
        }

        public static Collection<PSObject> RunScript(string scriptPath, Dictionary<string, string> arguments) {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            RunspaceInvoke runSpaceInvoker = new RunspaceInvoke(runspace);
            runSpaceInvoker.Invoke("Set-ExecutionPolicy Unrestricted");

            Pipeline pipeline = runspace.CreatePipeline();

            Command command = new Command(scriptPath);
            if (arguments != null) {
                foreach (var argument in arguments) {
                    command.Parameters.Add(argument.Key, argument.Value);
                }
            } 
            pipeline.Commands.Add(command);
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();
            return results;       
        }

        public static void Run(string scriptContents) {

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(scriptContents);
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();
            foreach (object result in results) {
                Console.WriteLine("Result: " + result);
            }
        }
    }
}
