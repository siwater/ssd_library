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
using System.Reflection;
using System.Xml;

using Citrix.Diagnostics;

namespace Citrix.SelfServiceDesktops.Agent {

    public class PsWrapper {

        private const string DebugPrefix = "[Debug]";

        private string scriptPath;
        private bool debug; 

        public PsWrapper(string scriptName) : this(scriptName, false) {}

        public PsWrapper(string scriptName, bool debug) {
            this.scriptPath = GetScriptPath(scriptName);
            this.debug = debug;
            IgnoreExceptions = new List<string>();
        }

        /// <summary>
        /// List of exceptions that may appear in the error output stream from the script that are expected and can
        /// safely be ignored (e.g. "ADIdentityNotFoundException"). Not matchin
        /// </summary>
        public List<string> IgnoreExceptions { get; private set; }


        public Collection<PSObject> RunPowerShell(Dictionary<string, object> arguments) {
            Command command = new Command(scriptPath);
            if (arguments != null) {
                foreach (var argument in arguments) {
                    command.Parameters.Add(argument.Key, argument.Value);
                }
            }
            if (debug) {
                command.Parameters.Add("debug");
            }
            try {
                PowerShell powerShell = PowerShell.Create();
                powerShell.Commands.AddCommand(command);
                Collection<PSObject> results = powerShell.Invoke();

                // The order is important here. Debug messages are flushed to the log *before* checking for errors
                // so the debug traces leading up to an error are not lost 
                Collection<PSObject> filteredResults = new Collection<PSObject>();
                foreach (PSObject result in results) {
                    string output = result.BaseObject as string;
                    if ((output != null) && output.StartsWith(DebugPrefix)) {
                        if (debug) {
                            CtxTrace.TraceInformation(output.Substring(DebugPrefix.Length));
                        }
                    } else {
                        filteredResults.Add(result);
                    }
                }
                foreach (DebugRecord r in powerShell.Streams.Debug) {
                    CtxTrace.TraceInformation(r.Message);
                }
                foreach (ErrorRecord r in powerShell.Streams.Error) {
                    // If the exception doesn't match a "to be ignored" exception, then throw it
                    if (IgnoreExceptions.SingleOrDefault(i =>
                        i.Equals(r.Exception.GetType().Name, StringComparison.InvariantCultureIgnoreCase)) == null) {
                        throw r.Exception;
                    }
                }
                return filteredResults;
            } catch (Exception e) {
                CtxTrace.TraceError(e);
                CtxTrace.TraceError(e.StackTrace);
                throw;
            }
        }

        private static string GetScriptPath(string inputScriptPath) {
            if (File.Exists(inputScriptPath)) {

                if (!Path.IsPathRooted(inputScriptPath)) {
                    return ".\\" + inputScriptPath;
                }
                return inputScriptPath;
            }

            string scriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), inputScriptPath);
            if (File.Exists(scriptPath)) {
                return scriptPath;
            }
            throw new System.Configuration.ConfigurationErrorsException("Unable to locate script " + inputScriptPath);
        }
    }
}
