using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Citrix.SelfServiceDesktops.DesktopModel;
using System.Security;

namespace Citrix.SelfServiceDesktops.WebApp
{
    public class MockDesktopManagerFactory : IDesktopManagerFactory
    {
        private Dictionary<string, IDesktopManager> mgrs = new Dictionary<string, IDesktopManager>();
        public MockDesktopManagerFactory()
        {
        }


        public string Name { get { return "Mock"; } }

        public IDesktopManager CreateManager(string user, SecureString password)
        {
            return null;
        }

        public IDesktopManager CreateManager(string user, string password)
        {
            IDesktopManager result;
            if (mgrs.TryGetValue(user, out result))
            {
                return result;
            }

            result = new MockDesktopManager();
            mgrs.Add(user, result);

            return result;
        }
    }
}