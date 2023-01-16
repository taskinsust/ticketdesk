// TicketDesk - Attribution notice
// Contributor(s):
//
//      Stephen Redd (https://github.com/stephenredd)
//
// This file is distributed under the terms of the Microsoft Public 
// License (Ms-PL). See http://opensource.org/licenses/MS-PL
// for the complete terms of use. 
//
// For any distribution that contains code from this file, this notice of 
// attribution must remain intact, and a copy of the license must be 
// provided to the recipient.

using Chart.Mvc.ComplexChart;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TicketDesk.Domain;
using TicketDesk.Domain.Model;
using TicketDesk.Localization;

namespace TicketDesk.Web.Client.Controllers
{
    [RoutePrefix("")]
    [Route("{action=index}")]
    public class HomeController : Controller
    {
        private TdDomainContext Context { get; set; }
        public HomeController(TdDomainContext context)
        {
            Context = context;
        }

        [Route("language")]
        public ActionResult SetLanguage(string name)
        {
            CultureHelper.SetCurrentCulture(name);

            var cookie = new HttpCookie("_culture", name);
            cookie.Expires = DateTime.Today.AddYears(1);
            Response.SetCookie(cookie);

            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            else
                return RedirectToAction("Index", "Home");
        }

        [Route("")]
        [Route("index")]
        public ActionResult Index()
        {
            if (ApplicationConfig.HomeEnabled)
            {
                //string options = "", options1 = "";
                //var lineChart = new Ticket().DrawLineChart(ref options);
                //var barChart = new Ticket().DrawBarChart();
                //var pieChart = new Ticket().DrawPieChart();
                //var doughnutChart = new Ticket().DrawDoughnutChart(ref options1);

                //ViewBag.lineChart = lineChart;
                //ViewBag.lineOptions = options;
                //ViewBag.doughtOptions = options1;
                //ViewBag.barChart = barChart;
                //ViewBag.pieChart = pieChart;
                //ViewBag.doughnutChart = doughnutChart;
                var ticketlastthirtyDays = Context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList().Count();
                var ticketLastsixtyDays = Context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-60) && x.TagList != "test,moretest").ToList().Count();
                var onlylastPrevThirtyDays = ticketLastsixtyDays - ticketlastthirtyDays;

                var diff = ticketlastthirtyDays - onlylastPrevThirtyDays;
                if (diff > 0)
                {
                    ViewBag.incpercentage = String.Format("{0:0.00}", ((double)diff / (double)ticketlastthirtyDays) * 100);
                }
                else
                {
                    ViewBag.decpercentage = String.Format("{0:0.00}", ((double)diff / (double)ticketlastthirtyDays) * 100);
                }

                // compare onlylastPrevThirtyDays & ticketlastthirtyDays
                ViewBag.totalticket = ticketlastthirtyDays;

                var lineChart = new Ticket().GetLineChartModel();
                ViewBag.lineChart = JsonConvert.SerializeObject(lineChart);

                var barChart = new Ticket().GetBarChartModel();
                ViewBag.barChart = JsonConvert.SerializeObject(barChart);

                var avgtimebarChart = new Ticket().GetAvgTimeBarChartModel();
                ViewBag.avgtimebarChart = JsonConvert.SerializeObject(avgtimebarChart);

                var totalticket = Context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();
                var pendingticket = totalticket.Where(x => x.TicketStatus == (int)0).ToList().Count();
                var moreInfoticket = totalticket.Where(x => (int)x.TicketStatus == (int)1).ToList().Count();
                var closedticket = totalticket.Where(x => (int)x.TicketStatus == (int)3).ToList().Count();

                ViewBag.totalticket = totalticket.Count();
                ViewBag.pendingticket = pendingticket;
                ViewBag.moreInfoticket = moreInfoticket;
                ViewBag.closedticket = closedticket;

                List<Ticket> ticketlist = totalticket.OrderByDescending(x => x.TicketId).Take(5).ToList();
                ViewBag.ticketlist = ticketlist;
                return View();
            }
            else
            {
                return RedirectToActionPermanent("Index", "TicketCenter");
            }
        }

        [Route("about")]
        public ActionResult About()
        {
            return View();
        }

        [Route("access-denied", Name = "AccessDenied")]
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}