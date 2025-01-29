using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MorpheusPoc.Models;

namespace MorpheusPoc.Controllers
{
    public static class RequestUrlExtensions
    {
        public static Uri ResolveRequestUrl(this HttpRequestBase request)
        {
            var forwardedHost = request.Headers["X-Forwarded-Host"];
            if (string.IsNullOrEmpty(forwardedHost))
            {
                return request.Url;
            }
            var forwardedProto = request.Headers["X-Forwarded-Proto"];
            var forwardedPort = int.Parse(request.Headers["X-Forwarded-Port"]);
            var forwardedPath = request.Headers["X-Forwarded-Path"];

            var uriBuilder = new UriBuilder(request.Url);
            uriBuilder.Scheme = forwardedProto;
            uriBuilder.Host = forwardedHost;
            uriBuilder.Port = forwardedPort;
            uriBuilder.Path = forwardedPath;
            
            return uriBuilder.Uri;
        }
        public static Uri ResolveRequestUrl(this HttpRequest request)
        {
            return ResolveRequestUrl(new HttpRequestWrapper(request));
        }
    }
    public class HomeController : Controller
    {
        private const string SIGNING_KEY = "su2Vj2GoduhHXlFMOvxh7iGXsJ1Iu1zw4NVerAE4elIN830TAsKzsAKtAJL1iV9B";

        public async Task<ActionResult> Index()
        {
            var model = new ViewRequestViewModel
                        {
                            RequestHeaders = GetValues(Request.Headers),
                            ResponseHeaders = GetValues(Response.Headers),
                            ServerVariables = GetValues(Request.ServerVariables)
                        };

            await ReadSignedHeader(model, "x-fl-userinfo");
            await ReadSignedHeader(model, "x-fl-impersonator");

            ViewBag.RequestUrl = Request.ResolveRequestUrl();
            
            return View(model);
        }

        private static async Task ReadSignedHeader(ViewRequestViewModel model, string headerName)
        {
            if (model.RequestHeaders.TryGetValue(headerName, out string headerValue))
            {

                var handler = new JsonWebTokenHandler();
                if (handler.CanReadToken(headerValue))
                {
                    
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_KEY));
                    
                    var parameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, 
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        RequireSignedTokens = true,
                        IssuerSigningKey = key,
                    };
                    
                    var result = await handler.ValidateTokenAsync(headerValue, parameters);
                    
                    if(result.IsValid)
                    {
                        foreach (KeyValuePair<string, object> claim in result.Claims)
                        {
                            model.AdditionalData.Add(headerName + "_" + claim.Key, claim.Value.ToString());
                        }
                    }
                }
            }
        }


        public async Task<ActionResult> MeshCall()
        {
            var meshTarget = ConfigurationManager.AppSettings["MeshTarget"];
            var model = new MeshCallViewModel();
            model.Target = meshTarget;

            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(meshTarget);
                var content = await response.Content.ReadAsStringAsync();
                model.StatusCode = response.StatusCode;
                model.Content = content;
            }
            catch (Exception e)
            {
                model.Error = e;
            }

            ViewBag.RequestUrl = Request.ResolveRequestUrl();

            return View(model);
        }

        private IDictionary<string, string> GetValues(NameValueCollection col)
        {
            var values = new Dictionary<string, string>();

            foreach (var key in col.AllKeys)
            {
                foreach (var value in col.GetValues(key))
                {
                    values.Add(key, value ?? "<NULL>");
                }
            }

            return values;
        }
    }
}