using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TicketDesk.Web.Client.Infrastructure.Filters
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "https://kudos.summitcommunications.net/");
            filterContext.RequestContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            //filterContext.RequestContext.HttpContext.Response.Flush();

            //filterContext.RequestContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "https://localhost:44343");
            //filterContext.RequestContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers",
            //"Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
            //filterContext.RequestContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            base.OnActionExecuting(filterContext);
        }
    }
}