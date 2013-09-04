/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Citrix.Diagnostics;
using Citrix.SelfServiceDesktops.DesktopLibrary;
using Citrix.SelfServiceDesktops.DesktopLibrary.Configuration;
using Citrix.SelfServiceDesktops.DesktopModel;

using CloudStack.SDK;

namespace TestApp {
    public class TestHarness {

        private List<string> operationsInProgress;

        public struct Account {
            public string Name;
            public string Password;
            public string Domain;
        }

        public struct SessionDefinition {
            public Cookie SessionCookie;
            public string SessionKey;
        }

        public readonly Uri WebAppUrl = new Uri("http://localhost/Citrix/SelfServiceDesktops");

        private Account AdminAccount = new Account() { Name = "admin", Password = "1pass@word1" };
        private Account UserAccount = new Account() { Name = "simon", Password = "pass@word1" };
        private Account DomainAccount = new Account() { Name = "simonw", Password = "pass@word1", Domain = "TestDomain" };

        public TestHarness() {
            CtxTrace.Initialize("self-service-desktops-test", true);
        }

        public void TestConfigurationReader(bool testRemote) {
            // Just read and report the local config
            Console.WriteLine("***Local Configuration****");
            IDesktopServiceConfiguration config = DesktopServiceConfiguration.ReadFrom(false);
            DisplayConfig(config);
            if (testRemote) {
                // Read from remote config if available
                Console.WriteLine("***Remote Configuration****");
                config = DesktopServiceConfiguration.ReadFrom(true);
                DisplayConfig(config);
            }
        }

        #region Public Display Methods

        public static void DisplayConfig(IDesktopServiceConfiguration config) {
            Console.WriteLine("Agent base Url is {0}", config.AgentUri);
            Console.WriteLine("Broker Url is {0}", config.BrokerUri);
            Console.WriteLine("CloudStack Url is {0}", config.CloudStackUri);
            DisplayOfferings(config.DesktopOfferings);
            Console.WriteLine("CloudStack Domain is {0}", config.Domain);
            Console.WriteLine("CloudStack HashCloudStackPassword is {0}", config.HashCloudStackPassword);
            Console.WriteLine("Agent ListenPort is {0}", config.ListenPort);
            DisplayPowerShellScript(config.PowerShellScript);
        }

        public static void DisplayPowerShellScript(IPowerShellScript script) {
            if (script == null) {
                Console.WriteLine("No PowerShellScript configured");
            } else {
                Console.WriteLine("PowerShellScript Path is {0}", script.Path);
                Console.WriteLine("PowerShellScript Frequency is {0}", script.Frequency);
                Console.WriteLine("PowerShellScript Debug is {0}", script.Debug);
            }
        }

        public static void DisplayOfferings(IEnumerable<IDesktopOffering> offerings) {
            foreach (IDesktopOffering offering in offerings) {
                Console.WriteLine("Desktop offering: {0}", offering);

                if (offering.DeviceCollection != null) {
                    Console.WriteLine("Device collection {0}", offering.DeviceCollection.Name);
                }
            }
        }

        #endregion

        public void TestDesktopManager() {
            TestDesktopManager(AdminAccount);
            TestDesktopManager(UserAccount);
            TestDesktopManager(DomainAccount);
        }

        public void TestSso() {
            TestSsoLocal(AdminAccount);
            TestSsoLocal(UserAccount);
            TestSsoLocal(DomainAccount);

            TestSsoRemote(AdminAccount, WebAppUrl);
            TestSsoRemote(UserAccount, WebAppUrl);
            TestSsoRemote(DomainAccount, WebAppUrl);
        }

        public void TestDesktopManager(Account user) {
            DesktopManagerFactory factory = new DesktopManagerFactory();
            IDesktopManager manager = (user.Domain == null) ? 
                factory.CreateManager(user.Name, user.Password) : factory.CreateManager(user.Name, user.Password, user.Domain);

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
                if (!operationInPogress(d.Id) && d.State == VirtualMachineState.Running) {
                    Console.WriteLine("Attempting to destroy {0}", d);
                    manager.DestroyDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            // Try start operation on a desktop     
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == VirtualMachineState.Stopped) {
                    Console.WriteLine("Attempting start of {0}", d);
                    manager.StartDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            // Try stop operation on a desktop
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == VirtualMachineState.Running) {
                    Console.WriteLine("Attempting stop of {0}", d);
                    manager.StopDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            // Try restart operation on a desktop
            foreach (IDesktop d in desktops) {
                if (!operationInPogress(d.Id) && d.State == VirtualMachineState.Running) {
                    Console.WriteLine("Attempting restart of {0}", d);
                    manager.RestartDesktop(d.Id);
                    operationsInProgress.Add(d.Id);
                    break;
                }
            }

            desktops = manager.ListDesktops();
            DisplayDesktops(desktops);
        }

        public void TestSsoLocal(Account user) {

            try {
                SessionDefinition session = GetSession(user);

                IDesktopManagerFactory factory = new DesktopManagerFactory();
                string sessionKey = session.SessionKey;
                string jsessionId = session.SessionCookie.Value;
                Console.WriteLine("Create desktop manager with sessionkey {0}, jsessionid {1}", sessionKey, jsessionId );
                IDesktopManager manager = factory.CreateManager(user.Name, sessionKey, jsessionId, user.Domain);
                IEnumerable<IDesktop> desktops = manager.ListDesktops();
                Console.WriteLine("Enumerated desktops");

            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public void TestSsoRemote(Account user, Uri webAppUrl) {
            try {
                SessionDefinition session = GetSession(user);
                UriBuilder uriBuilder = new UriBuilder(webAppUrl);
                string sessionKey = HttpUtility.UrlEncode(session.SessionKey);
                Console.WriteLine("Session key (encoded): {0}", sessionKey);
                string query = string.Format("username={0}&sessionkey={1}&jsessionid={2}", user.Name, sessionKey, session.SessionCookie.Value);
                uriBuilder.Query = query;
                Console.WriteLine("Attempting logon to web app: {0}", uriBuilder.ToString());
                string response = SendRequest(uriBuilder.Uri);
                // Ahem - rather crude way to detect the logon succeeded
                if (response.Contains("Desktops for")) {
                    Console.WriteLine("Successfully logged on to WebApp");
                } else {
                    Console.WriteLine("Sso Failed, Response is:");
                    Console.WriteLine(response);
                    throw new ApplicationException("WebApp SSO Failed");
                }

            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        #region Private Methods

        private SessionDefinition GetSession(Account user) {

            IDesktopServiceConfiguration config = DesktopServiceConfiguration.Read();
            Client client = new Client(config.CloudStackUri);
            client.Login(user.Name, user.Password, user.Domain, config.HashCloudStackPassword);
            Console.WriteLine("Logged into Cloudstack as {0}", user.Name);
            ListVirtualMachines(client);

            Cookie cookie = FindSessionCookie(client.Cookies);
            if (cookie == null) {
                throw new ApplicationException("Failed to acquire session: Could not find session cookie");
            }
            ShowCookie(cookie);
            Console.WriteLine("Session Key is {0}", client.SessionKey);
            return new SessionDefinition() { SessionCookie = cookie, SessionKey = client.SessionKey };
        }

        private string SendRequest(Uri uri) {

            HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
            httpWebRequest.CookieContainer = new CookieContainer();
            httpWebRequest.Method = "GET";
            using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse) {
                using (Stream respStrm = httpWebResponse.GetResponseStream()) {
                    using (StreamReader streamReader = new StreamReader(respStrm, Encoding.UTF8)) {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }

        private void ShowCookie(Cookie cookie) {
            Console.WriteLine("Cookie.Name: {0}, Cookie.Value: {1}", cookie.Name, cookie.Value);
            Console.WriteLine("Cookie.Domain: {0}, Coookie.Path={1}", cookie.Domain, cookie.Path);
        }

        private Cookie FindSessionCookie(CookieCollection cookieCollection) {
            foreach (Cookie c in cookieCollection) {
                if (c.Name == "JSESSIONID") {
                    return c;
                }
            }
            return null;
        }

        private void ListVirtualMachines(Client client) {
            ListVirtualMachinesRequest request = new ListVirtualMachinesRequest();
            //request.Parameters["listall"] = "true";
            ListVirtualMachinesResponse response = client.ListVirtualMachines(request);
            Console.WriteLine("Got {0} virtual machines", response.VirtualMachine.Length);
        }

        private bool operationInPogress(string id) {
            return operationsInProgress.Contains(id);
        }

        private void DisplayDesktops(IEnumerable<IDesktop> desktops) {
            Console.WriteLine("There are {0} desktops", desktops.Count());
            foreach (IDesktop desktop in desktops) {
                Console.WriteLine("Got desktop : {0}", desktop);
                Console.WriteLine("Desktop state: {0}", desktop.DesktopState);
            }
        }

   

        #endregion
    }
}
