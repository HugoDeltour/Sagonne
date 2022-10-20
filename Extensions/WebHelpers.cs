using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Principal;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Helpers
{
    public static class WebHelpers
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static IHostingEnvironment Environment { get; private set; }

        public static void Configure(IHttpContextAccessor httpContextAccessor, IHostingEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor;
            Environment = env;
        }

        public static bool IsStaging => Environment.IsStaging();

        public static bool IsEnvironment(string environmentName)
        {
            return Environment.IsEnvironment(environmentName);
        }

        public static HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public static string GetRemoteIP
        {
            get { return HttpContext.Connection.RemoteIpAddress.ToString(); }
        }

        public static string GetUserAgent
        {
            get { return HttpContext.Request.Headers["User-Agent"].ToString(); }
        }

        public static string GetScheme
        {
            get { return HttpContext.Request.Scheme; }
        }

        public static HostString GetHost
        {
            get { return HttpContext.Request.Host; }
        }

        public static string GetPath
        {
            get { return HttpContext.Request.Path; }
        }

        public static string GetQueryString
        {
            get { return HttpContext.Request.QueryString.ToString(); }
        }

        public static Uri GetReferer
        {
            get => HttpContext.Request.GetTypedHeaders().Referer;
        }

        public static Dictionary<string, string> GetUrlParameters
        {
            get
            {
                string[] arrParams = HttpContext.Request.QueryString.Value.Replace("?", string.Empty).Split("&");

                if (arrParams.Any(p => string.IsNullOrEmpty(p)))
                    return new Dictionary<string, string>();

                return !arrParams.Any() ? new Dictionary<string, string>() : arrParams.ToDictionary(k => k.Split("=")[0], v => v.Split("=")[1]);
            }
        }

        public static string GetUserName
        {
            get
            {
                string identity = string.Empty;

                if (HttpContext.User.Identity is ClaimsIdentity claims)
                    identity = claims.Claims.FirstOrDefault(c => c.Type == "username")?.Value;

                if (string.IsNullOrEmpty(identity))
                    identity = WindowsIdentity.GetCurrent().Name;

                return identity.Substring(identity.IndexOf(@"\") + 1).ToUpper();
            }
        }

    }
}
