using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace MorpheusPoc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new ViewRequestViewModel
                        {
                            RequestHeaders = GetValues(Request.Headers),
                            ResponseHeaders = GetValues(Response.Headers),
                            ServerVariables = GetValues(Request.ServerVariables)
                        };

            model.AdditionalData.Add(nameof(Request.UserHostAddress), Request.UserHostAddress);

            model.AdditionalData.Add("Current Server Local Time", DateTimeOffset.Now.ToString());

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