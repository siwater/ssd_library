using Citrix.SelfServiceDesktops.DesktopModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Citrix.SelfServiceDesktops.WebApp
{
    /// <summary>
    /// Description for how to write an AuthorizeAttribute posted on line
    /// See http://cacheandquery.com/blog/2011/03/customizing-asp-net-mvc-basic-authentication/, 
    /// which may borrow from the similiar http://stackoverflow.com/a/1411131/939250
    /// MSDN have their own tips at 
    /// http://msdn.microsoft.com/en-us/library/dd460317%28v=vs.108%29.aspx#deriving_from_authorizeattribute
    /// Basic authentication is described in RFC 1945 http://tools.ietf.org/html/rfc1945#section-11.1
    /// </summary>
    /// <remarks>
    /// Disable Forms authentication.  It's not as easy as it sounds,
    /// see http://blog.alexonasp.net/post/2011/08/23/Basic-Authentication-with-WCF-Web-API-hosted-in-IIS-Getting-a-404-Disable-Forms-Authentication-Redirection.aspx
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BasicAuthenticationAttribute : AuthorizeAttribute
    {
        bool forceSsl = false;

        /// <summary>
        /// Used to force non-local connections over HTTP to fail.
        /// </summary>
        public bool ForceSsl
        {
            get { return forceSsl; }
            set { forceSsl = value; }
        }

        public BasicAuthenticationAttribute() { }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            // Authenticate validates against an identity repository, AuthorizeCore cross references users/roles specified when the attribute is used.
            if (Authenticate(filterContext.HttpContext) && AuthorizeCore(filterContext.HttpContext))
            {
                HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
                cachePolicy.SetProxyMaxAge(new TimeSpan(0));
                cachePolicy.AddValidationCallback(CacheValidateHandler, null /* data */);
            }
            else
            {
                filterContext.Result = new HttpBasicAuthenticationUnauthorizedResult();
            }
            
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }

        private bool Authenticate(HttpContextBase context)
        {
            // SSL requirements met?
            if (forceSsl && !context.Request.IsSecureConnection && !context.Request.IsLocal)
            {
                return false;
            }
            // Authorization header present?
            if (!context.Request.Headers.AllKeys.Contains("Authorization"))
            {
                return false;
            }

            string authHeader = context.Request.Headers["Authorization"];
            IPrincipal principal;
            if (TryGetPrincipal(authHeader, out principal))
            {
                HttpContext.Current.User = principal;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Authorization header to IPrincipal in 'TryGet' idiom
        /// </summary>
        /// <param name="authHeader"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        private bool TryGetPrincipal(string authHeader, out IPrincipal principal)
        {
            var credentials = ParseAuthorizeHeader(authHeader);
            if (credentials != null && TryGetPrincipal(credentials[0], credentials[1], out principal))
            {
                return true;
            }
            principal = null;
            return false;
        }


        /// <summary>
        /// Decode "Authorization" header into [ user, password ]
        /// </summary>
        /// <param name="authHeader"></param>
        /// <returns></returns>
        private string[] ParseAuthorizeHeader(string authHeader)
        {
            // screen for plausibility
            if (authHeader == null || authHeader.Length == 0 || !authHeader.StartsWith("Basic"))
            {
                return null;
            }

            // Credentials Base64 encoded and separated by ':', see http://tools.ietf.org/html/rfc1945#section-11.1
            string base64Credentials = authHeader.Substring("Basic".Length + 1);
            string decodedCredentials = Encoding.ASCII.GetString(Convert.FromBase64String(base64Credentials));
            string[] credentials = decodedCredentials.Split(new char[] { ':' });

            // screen for plausibility
            if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[0])) 
            {
                return null;
            }
            return credentials;
        }

        /// <summary>
        /// Username / password to IPrincipal in 'TryGet' idiom
        /// </summary>
        private bool TryGetPrincipal(string userName, string password, out IPrincipal principal)
        {
            IDesktopManagerFactory factory = HttpContext.Current.Application.Get("IDesktopManagerFactory") as IDesktopManagerFactory;
            if (userName != null && factory != null)
            {
                IDesktopManager mgr = factory.CreateManager(userName, password);

                if (mgr != null)
                {
                    HttpContext.Current.Session.Add("IDesktopManager", mgr);
                    principal = new GenericPrincipal(new GenericIdentity(userName), new string[] { "user" });
                    
                    return true;
                }
            }
            principal = null;
            return false;
        }
    }

    /// <summary>
    /// 401 with WWW-Authenticate header to trigger password request
    /// </summary>
    public class HttpBasicAuthenticationUnauthorizedResult : HttpUnauthorizedResult
    {
        public HttpBasicAuthenticationUnauthorizedResult() : base() { }
        public HttpBasicAuthenticationUnauthorizedResult(string statusDescription) : base(statusDescription) { }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            // Sending a WWW-Authenticate header with value "Basic" triggers
            // credentials request in client browesr.
            var basicAuthChallenge = String.Format("Basic realm=\"{0}\"", "Simple Self-Service Desktop");
            context.HttpContext.Response.AddHeader("WWW-Authenticate", basicAuthChallenge);

            base.ExecuteResult(context);
        }
    }
}
