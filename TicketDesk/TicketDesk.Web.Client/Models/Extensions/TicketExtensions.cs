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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ganss.XSS;
using TicketDesk.IO;
using TicketDesk.Search.Common;
using TicketDesk.Web.Client;
using TicketDesk.Web.Client.Models;
using TicketDesk.Web.Identity;
using TicketDesk.Web.Identity.Model;
using TicketDesk.Localization.Models;
using Chart.Mvc.ComplexChart;
using Chart.Mvc.SimpleChart;

namespace TicketDesk.Domain.Model
{
    public static class TicketExtensions
    {
        public static IEnumerable<SearchIndexItem> ToSeachIndexItems(this IEnumerable<Ticket> tickets)
        {
            return tickets.Select(t => new SearchIndexItem
            {
                Id = t.TicketId,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Details = t.Details,
                Status = t.TicketStatus.ToString(),
                LastUpdateDate = t.LastUpdateDate,
                Tags = string.IsNullOrEmpty(t.TagList) ? new string[] { } : t.TagList.Split(','),
                //not null comments only, otherwise we end up indexing empty array item, or blowing up azure required field
                Events = t.TicketEvents.Where(c => !string.IsNullOrEmpty(c.Comment)).Select(c => c.Comment).ToArray()
            });
        }

        public static UserDisplayInfo GetAssignedToInfo(this Ticket ticket)
        {
            return GetUserInfo(ticket.AssignedTo);
        }

        public static UserDisplayInfo GetCreatedByInfo(this Ticket ticket)
        {
            return GetUserInfo(ticket.CreatedBy);
        }

        public static UserDisplayInfo GetOwnerInfo(this Ticket ticket)
        {
            return GetUserInfo(ticket.Owner);
        }

        public static UserDisplayInfo GetLastUpdatedByInfo(this Ticket ticket)
        {
            return GetUserInfo(ticket.LastUpdateBy);
        }

        public static UserDisplayInfo GetCurrentStatusSetByInfo(this Ticket ticket)
        {
            return GetUserInfo(ticket.CurrentStatusSetBy);
        }

        public static UserDisplayInfo GetUserInfo(string userId)
        {
            var userManager = DependencyResolver.Current.GetService<TicketDeskUserManager>();
            return userManager.GetUserInfo(userId);
        }

        public static HtmlString HtmlDetails(this Ticket ticket)
        {
            var content = (ticket.IsHtml) ? ticket.Details : ticket.Details.HtmlFromMarkdown();
            var san = new HtmlSanitizer();
            return new HtmlString(san.Sanitize(content));
        }

        public static SelectList GetPriorityList(this Ticket ticket)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return context.TicketDeskSettings.GetPriorityList(true, ticket.Priority);
        }

        public static SelectList GetCategoryList(this Ticket ticket)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return context.TicketDeskSettings.GetCategoryList(true, ticket.Category);
        }

        public static SelectList GetTicketTypeList(this Ticket ticket)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return context.TicketDeskSettings.GetTicketTypeList(true, ticket.TicketType);
        }

        public static SelectList GetProjectList(this Ticket ticket, int? selectedProject)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return context.Projects.OrderBy(p => p.ProjectName)
                .ToSelectList(p => p.ProjectId.ToString(), p => p.ProjectName, selectedProject ?? 0, true);
        }

        public static SelectList GetOwnersList(this Ticket ticket, bool excludeCurrentUser = false, bool excludeCurrentOwner = false)
        {
            var roleManager = DependencyResolver.Current.GetService<TicketDeskRoleManager>();
            var userManager = DependencyResolver.Current.GetService<TicketDeskUserManager>();
            var sec = DependencyResolver.Current.GetService<TicketDeskContextSecurityProvider>();
            IEnumerable<TicketDeskUser> all = roleManager.GetTdInternalUsers(userManager);
            if (excludeCurrentUser)
            {
                all = all.Where(u => u.Id != sec.CurrentUserId);
            }
            if (excludeCurrentOwner)
            {
                all = all.Where(u => u.Id != ticket.Owner);
            }
            return all.ToUserSelectList(false, ticket.Owner);
        }


        public static SelectList GetAssignedToList(this Ticket ticket, bool excludeCurrentUser = false, bool excludeCurrentAssignedTo = false, bool includeEmptyText = true)
        {
            var roleManager = DependencyResolver.Current.GetService<TicketDeskRoleManager>();
            var userManager = DependencyResolver.Current.GetService<TicketDeskUserManager>();
            var sec = DependencyResolver.Current.GetService<TicketDeskContextSecurityProvider>();
            IEnumerable<TicketDeskUser> all = roleManager.GetTdHelpDeskUsers(userManager);
            if (excludeCurrentUser)
            {
                all = all.Where(u => u.Id != sec.CurrentUserId);
            }
            if (excludeCurrentAssignedTo)
            {
                all = all.Where(u => u.Id != ticket.AssignedTo);
            }
            return includeEmptyText ? all.ToUserSelectList(ticket.AssignedTo, Strings.AssignedTo_Unassigned) : all.ToUserSelectList(false, ticket.AssignedTo);
        }

        public static bool AllowEditTags(this Ticket ticket)
        {
            //TODO: is this the best place to put this check?
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return context.SecurityProvider.IsTdHelpDeskUser || context.TicketDeskSettings.Permissions.AllowInternalUsersToEditTags;
        }

        public static bool AllowSetOwner(this Ticket ticket)
        {
            //TODO: is this the best place to put this check? Is this one even worth the extension?
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return (context.SecurityProvider.IsTdInternalUser && context.TicketDeskSettings.Permissions.AllowInternalUsersToSetOwner) || (context.SecurityProvider.IsTdHelpDeskUser || context.SecurityProvider.IsTdAdministrator);
        }

        public static bool AllowSetAssigned(this Ticket ticket)
        {
            //TODO: is this the best place to put this check? Is this one even worth the extension?
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return (context.SecurityProvider.IsTdInternalUser && context.TicketDeskSettings.Permissions.AllowInternalUsersToSetAssigned) || (context.SecurityProvider.IsTdHelpDeskUser || context.SecurityProvider.IsTdAdministrator);

        }

        public static bool AllowEditPriorityList(this Ticket ticket)
        {
            //TODO: is this the best place to put this check?
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            return (context.SecurityProvider.IsTdAdministrator || context.SecurityProvider.IsTdHelpDeskUser) || (context.SecurityProvider.IsTdInternalUser && context.TicketDeskSettings.Permissions.AllowInternalUsersToEditPriority);
        }

        /// <summary>
        /// Commits any pending attachments for the ticket in the file store.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="tempId">The temporary identifier for pending attachments.</param>
        /// <returns>IEnumerable&lt;System.String&gt;. The list of filenames for all attachments saved</returns>
        public static IEnumerable<string> CommitPendingAttachments(this Ticket ticket, Guid tempId)
        {
            var attachments = TicketDeskFileStore.ListAttachmentInfo(tempId.ToString(), true).ToArray();
            foreach (var attachment in attachments)
            {
                TicketDeskFileStore.MoveFile(
                    attachment.Name,
                    tempId.ToString(),
                    ticket.TicketId.ToString(CultureInfo.InvariantCulture),
                    true,
                    false);
            }
            return attachments.Select(a => a.Name);
        }


        #region Chart Js Implementation from https://github.com/martinobordin/Chart.Mvc

        public static LineChart DrawLineChart(this Ticket ticket, ref string options)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            var totalticket = context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();

            // get the data from 
            var linechart = new LineChart();

            var data = from e in totalticket
                       group e by e.CreatedDate.Date.ToShortDateString() into g
                       select new { g.Key, Ticket = g.Count() };
            var labels = data.Select(x => x.Key).ToList<string>();
            var count = data.Select(x => (double)x.Ticket).ToList<double>();

            linechart.ComplexData.Labels.AddRange(labels);

            options = @"          {
                                    layout: {
                                      padding: {                
                                        top: 20                
                                      }
                                    },
                                    scales: {
                                        xAxes: [
                                            {
                                            beginAtZero: true,
                                                ticks: {
                                                autoSkip: false
                                                }
                                        }
                                        ]
                                    }
                                }";

            var closeData = totalticket.Where(x => x.TicketStatus == TicketStatus.Closed).OrderBy(x => x.LastUpdateDate).ToList();
            var data1 = from e in closeData
                        group e by e.LastUpdateDate.Date.ToShortDateString() into g
                        select new { g.Key, Ticket = g.Count() };

            linechart.ComplexData.Datasets.AddRange(new List<ComplexDataset>() {
            new ComplexDataset()
            {
                    Label = "TTT",
                    Data = data.Select(x => (double)x.Ticket).ToList<double>(),
                    FillColor = "rgb(229,173,20)",
                    StrokeColor = "rgb(229,173,20,1)",
                    PointColor = "rgb(229,173,20, 1)",
                    PointStrokeColor = "#fff",
                    PointHighlightFill = "#fff",
                    PointHighlightStroke = "rgb(229,173,20,1)"
            }
            ,
            new ComplexDataset
            {
                    Label = "YYY",
                    Data = data1.Select(x => (double)x.Ticket).ToList<double>(),
                    FillColor = "rgb(94,192,190)",
                    StrokeColor = "rgb(94,192,190,.5)",
                    PointColor = "rgb(94,192,190, 1)",
                    PointStrokeColor = "#fff",
                    PointHighlightFill = "#fff",
                    PointHighlightStroke = "rgb(94,192,190,1)",
             }
            });
            return linechart;
        }

        public static BarChart DrawBarChart(this Ticket ticket)
        {
            var barchart = new BarChart();
            var utility = new Utility();
            var result = utility.LoadAvgClosingTime();
            result = result.OrderBy(x => x.ClosingDate).ToList();
            var labels = result.Select(x => x.ClosingDate.Date.ToShortDateString()).ToList<string>();
            barchart.ComplexData.Labels.AddRange(labels);
            barchart.ComplexData.Datasets.AddRange(new List<ComplexDataset>() {

            new ComplexDataset()
            {
                    Data = result.Select(x => (double)x.AVGHour).ToList<double>(),
                    Label = "AVG ticket closing time",
                    FillColor = "rgb(21,145,165, 0.2)" ,
                    StrokeColor = "rgba(220,220,220,1)",
                    PointColor = "rgb(21,145,165, 1)",
                    PointStrokeColor = "#fff",
                    PointHighlightFill = "#fff",
                    PointHighlightStroke = "rgba(220,220,220,1)"
            }});

            return barchart;
        }

        public static PieChart DrawPieChart(this Ticket ticket)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            var totalticket = context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();

            // get the data from 
            var piechart = new PieChart();
            var data = from e in totalticket
                       group e by e.TicketStatus into g
                       select new { g.Key, Ticket = g.Count() };
            List<SimpleData> simpleList = new List<SimpleData>();
            string[] color = new string[] { "#46BFBD", "#FDB45C" };
            string[] highlight = new string[] { "#5AD3D1", "#FFC870" };
            int i = 0;
            foreach (var item in data)
            {
                simpleList.Add(new SimpleData() { Label = item.Key.ToString(), Value = item.Ticket, Color = color[i], Highlight = highlight[i] });
                i++;
                if (i > 1)
                {
                    i = 0;
                }
            }
            piechart.Data = simpleList;
            return piechart;
        }
        public static DoughnutChart DrawDoughnutChart(this Ticket ticket, ref string options1)
        {
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            var totalticket = context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();

            options1 = @" {
           tooltips: {
         enabled: false
    },
             plugins: {
            datalabels: {
                formatter: (value, ctx) => {
                  let sum = 0;
                  let dataArr = ctx.chart.data.datasets[0].data;
                  dataArr.map(data => {
                      sum += data;
                  });
                  let percentage = (value*100 / sum).toFixed(2)+' % ';
                  return percentage;
        },
                color: '#fff',
            }
    }
    }; ";

            // get the data from 
            var doughnutChart = new DoughnutChart();
            var data = from e in totalticket
                       group e by e.TicketStatus into g
                       select new { g.Key, Ticket = g.Count() };
            List<SimpleData> simpleList = new List<SimpleData>();
            string[] color = new string[] { "#46BFBD", "#FDB45C" };
            string[] highlight = new string[] { "#5AD3D1", "#FFC870" };
            int i = 0;
            foreach (var item in data)
            {
                simpleList.Add(new SimpleData() { Label = item.Key.ToString(), Value = item.Ticket, Color = color[i], Highlight = highlight[i] });
                i++;
                if (i > 1)
                {
                    i = 0;
                }
            }
            doughnutChart.Data = simpleList;
            return doughnutChart;
        }
        #endregion

        #region Chart update version Implementation

        public static ChartModel GetLineChartModel(this Ticket ticket)
        {
            ChartModel chartModel = new ChartModel();
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            var totalticket = context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();

            var openData = from e in totalticket
                           group e by e.CreatedDate.Date.ToShortDateString() into g
                           select new { g.Key, Ticket = g.Count() };

            var closeTicket = totalticket.Where(x => x.TicketStatus == TicketStatus.Closed).OrderBy(x => x.LastUpdateDate).ToList();
            var closeData = from e in closeTicket
                            group e by e.LastUpdateDate.Date.ToShortDateString() into g
                            select new { g.Key, Ticket = g.Count() };

            var labels = openData.Select(x => x.Key).ToList<string>();
            chartModel.labels = labels;

            chartModel.datasets.AddRange(new List<Dataset>() {
                new Dataset{
                    type = "line",
                    pointBorderColor= "#007bff",
                    pointBackgroundColor ="#007bff",
                    backgroundColor= "#007bff",
                    borderColor = "#007bff",
                    data =openData.Select(x => (int)x.Ticket).ToList<int>(),
                },
                new Dataset{
                    type = "line",
                    pointBorderColor= "#ced4da",
                    pointBackgroundColor ="#ced4da",
                    backgroundColor= "#ced4da",
                    borderColor = "#ced4da",
                    data =closeData.Select(x => (int)x.Ticket).ToList<int>(),}
            });

            return chartModel;
        }
        public static ChartModel GetBarChartModel(this Ticket ticket)
        {
            ChartModel chartModel = new ChartModel();
            var context = DependencyResolver.Current.GetService<TdDomainContext>();
            var totalticket = context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();

            var openData = from e in totalticket
                           group e by e.CreatedDate.Date.ToShortDateString() into g
                           select new { g.Key, Ticket = g.Count() };

            var closeTicket = totalticket.Where(x => x.TicketStatus == TicketStatus.Closed).OrderBy(x => x.LastUpdateDate).ToList();
            var closeData = from e in closeTicket
                            group e by e.LastUpdateDate.Date.ToShortDateString() into g
                            select new { g.Key, Ticket = g.Count() };

            var labels = openData.Select(x => x.Key).ToList<string>();
            chartModel.labels = labels;

            chartModel.datasets.AddRange(new List<Dataset>() {
                new Dataset{
                    //type = "line",
                    pointBorderColor= "#007bff",
                    pointBackgroundColor ="#007bff",
                    backgroundColor= "#007bff",
                    borderColor = "#007bff",
                    data =openData.Select(x => (int)x.Ticket).ToList<int>(),
                },
                new Dataset{
                    //type = "line",
                    pointBorderColor= "#ced4da",
                    pointBackgroundColor ="#ced4da",
                    backgroundColor= "#ced4da",
                    borderColor = "#ced4da",
                    data =closeData.Select(x => (int)x.Ticket).ToList<int>(),}
            });

            return chartModel;
        }

        public static ChartModel GetAvgTimeBarChartModel(this Ticket ticket)
        {
            ChartModel chartModel = new ChartModel();
            var context = DependencyResolver.Current.GetService<TdDomainContext>();

            var utility = new Utility();
            var result = utility.LoadAvgClosingTime();
            result = result.OrderBy(x => x.ClosingDate).ToList();
            var labels = result.Select(x => x.ClosingDate.Date.ToShortDateString()).ToList<string>();
            chartModel.labels = labels;
            chartModel.datasets.AddRange(new List<Dataset>() {
                new Dataset{

                    pointBorderColor= "#007bff",
                    pointBackgroundColor ="#007bff",
                    backgroundColor= "#007bff",
                    borderColor = "#007bff",
                    data =result.Select(x => (int)x.AVGHour).ToList<int>()
                }
            });

            return chartModel;
        }

        #endregion
    }

}