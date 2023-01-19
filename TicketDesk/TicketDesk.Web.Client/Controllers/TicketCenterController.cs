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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TicketDesk.Domain;
using TicketDesk.Domain.Model;
using TicketDesk.Web.Client.Models;

namespace TicketDesk.Web.Client.Controllers
{
    [RoutePrefix("tickets")]
    [Route("{action=index}")]
    [TdAuthorize(Roles = "TdInternalUsers,TdHelpDeskUsers,TdAdministrators")]
    //[OutputCacheAttribute(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class TicketCenterController : Controller
    {
        private TdDomainContext Context { get; set; }
        public TicketCenterController(TdDomainContext context)
        {
            Context = context;
        }

        [Route("reset-user-lists")]
        public async Task<ActionResult> ResetUserLists()
        {
            var uId = Context.SecurityProvider.CurrentUserId;
            await Context.UserSettingsManager.ResetAllListSettingsForUserAsync(uId);
            var x = await Context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        // GET: TicketCenter
        [Route("{listName?}/{page:int?}")]
        public async Task<ActionResult> Index(int? page, string listName)
        {
            listName = listName ?? (Context.SecurityProvider.IsTdHelpDeskUser ? "unassigned" : "mytickets");
            var pageNumber = page ?? 1;

            var viewModel = await TicketCenterListViewModel.GetViewModelAsync(pageNumber, listName, Context, Context.SecurityProvider.CurrentUserId);//new TicketCenterListViewModel(listName, model, Context, User.Identity.GetUserId());
            Response.AddHeader("Refresh", "300");
            return View(viewModel);
        }

        //[Route("{listName?}/{page:int?}")]
        public async Task<ActionResult> TicketCenterDatasets(int? page, string listName)
        {
            listName = listName ?? (Context.SecurityProvider.IsTdHelpDeskUser ? "unassigned" : "mytickets");
            var pageNumber = page ?? 1;

            var viewModel = await TicketCenterListViewModel.GetViewModelAsync(pageNumber, listName, Context, Context.SecurityProvider.CurrentUserId);//new TicketCenterListViewModel(listName, model, Context, User.Identity.GetUserId());
            Response.AddHeader("Refresh", "300");
            return View(viewModel);
        }

        [Route("pageList/{listName=mytickets}/{page:int?}")]
        public async Task<ActionResult> PageList(int? page, string listName)
        {
            return await GetTicketListPartial(page, listName);
        }

        [Route("filterList/{listName=opentickets}/{page:int?}")]
        public async Task<PartialViewResult> FilterList(
            string listName,
            int pageSize,
            string ticketStatus,
            string owner,
            string assignedTo,
            string fromDate,
            string toDate)
        {
            var uId = Context.SecurityProvider.CurrentUserId;
            var userSetting = await Context.UserSettingsManager.GetSettingsForUserAsync(uId);

            var currentListSetting = userSetting.GetUserListSettingByName(listName);

            currentListSetting.ModifyFilterSettings(pageSize, ticketStatus, owner, assignedTo);

            await Context.SaveChangesAsync();

            return await GetTicketListPartial(null, listName);

        }

        [Route("sortList/{listName=opentickets}/{page:int?}")]
        public async Task<PartialViewResult> SortList(
            int? page,
            string listName,
            string columnName,
            bool isMultiSort = false)
        {
            var uId = Context.SecurityProvider.CurrentUserId;
            var userSetting = await Context.UserSettingsManager.GetSettingsForUserAsync(uId);
            var currentListSetting = userSetting.GetUserListSettingByName(listName);

            var sortCol = currentListSetting.SortColumns.SingleOrDefault(sc => sc.ColumnName == columnName);

            if (isMultiSort)
            {
                if (sortCol != null)// column already in sort, remove from sort
                {
                    if (currentListSetting.SortColumns.Count > 1)//only remove if there are more than one sort
                    {
                        currentListSetting.SortColumns.Remove(sortCol);
                    }
                }
                else// column not in sort, add to sort
                {
                    currentListSetting.SortColumns.Add(new UserTicketListSortColumn(columnName, ColumnSortDirection.Ascending));
                }
            }
            else
            {
                if (sortCol != null)// column already in sort, just flip direction
                {
                    sortCol.SortDirection = (sortCol.SortDirection == ColumnSortDirection.Ascending) ? ColumnSortDirection.Descending : ColumnSortDirection.Ascending;
                }
                else // column not in sort, replace sort with new simple sort for column
                {
                    currentListSetting.SortColumns.Clear();
                    currentListSetting.SortColumns.Add(new UserTicketListSortColumn(columnName, ColumnSortDirection.Ascending));
                }
            }

            await Context.SaveChangesAsync();

            return await GetTicketListPartial(page, listName);
        }

        private async Task<PartialViewResult> GetTicketListPartial(int? page, string listName)
        {
            var pageNumber = page ?? 1;

            var viewModel = await TicketCenterListViewModel.GetViewModelAsync(pageNumber, listName, Context, Context.SecurityProvider.CurrentUserId);
            return PartialView("_TicketList", viewModel);

        }

        public PartialViewResult LoadDashboard()
        {

            var totalticket = Context.Tickets.ToList().Where(x => x.CreatedDate > DateTime.Now.AddDays(-30) && x.TagList != "test,moretest").ToList();
            var pendingticket = totalticket.Where(x => x.TicketStatus == (int)0).ToList().Count();
            var moreInfoticket = totalticket.Where(x => (int)x.TicketStatus == (int)1).ToList().Count();
            var closedticket = totalticket.Where(x => (int)x.TicketStatus == (int)3).ToList().Count();

            ViewBag.totalticket = totalticket.Count();
            ViewBag.pendingticket = pendingticket;
            ViewBag.moreInfoticket = moreInfoticket;
            ViewBag.closedticket = closedticket;

            var totalticketToday = Context.Tickets.ToList().Where(x => DateTime.Compare(x.CreatedDate.Date, DateTime.Now.Date) == 0 && x.TagList != "test,moretest").ToList();
            var pendingticketToday = totalticketToday.Where(x => (int)x.TicketStatus == 0).ToList().Count();
            var moreInfoticketToday = totalticketToday.Where(x => (int)x.TicketStatus == 1).ToList().Count();
            var closedticketToday = Context.Tickets.ToList().Where(x => (int)x.TicketStatus == 3 && DateTime.Compare(x.CurrentStatusDate.Date, DateTime.Now.Date) == 0).ToList().Count();

            ViewBag.totalticketToday = totalticketToday.Count();
            ViewBag.pendingticketToday = pendingticketToday;
            ViewBag.moreInfoticketToday = moreInfoticketToday;
            ViewBag.closedticketToday = closedticketToday;
            return PartialView("_dashboard");
        }

        public ActionResult TopDelayedTask()
        {

            return View();
        }

        [HttpPost]
        public ActionResult TopDelayedTaskDatasets(int draw = 0, int start = 0, int length = 0, string searchvalue = "")
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    Utility utility = new Utility();
                    var delayedList = utility.LoadTopDelayedTask(start, length);
                    int recordsTotal = utility.CountTopDelayedTask();

                    long recordsFiltered = recordsTotal;

                    var data = new List<object>();

                    int sl = start + 1;

                    foreach (var d in delayedList)
                    {
                        var str = new List<string>();

                        //str.Add(sl.ToString());
                        str.Add(d.TicketId);
                        str.Add(d.TicketTitle);

                        //var ss = (int)TicketStatus.Closed;

                        if (Convert.ToInt32(d.Status) == (int)TicketStatus.Closed)
                            str.Add(d.CompletionTime);
                        else str.Add("");

                        str.Add(d.TicketOwner);
                        str.Add(d.CreatedAt.ToString("d"));

                        if (Convert.ToInt32(d.Status) == (int)TicketStatus.Active)
                        {
                            str.Add("");
                            str.Add("");
                        }
                        else
                        {
                            str.Add(d.TicketCLosedBy);
                            str.Add(d.TicketCLosingDate.ToString("d"));
                        }

                        if (Convert.ToInt32(d.Status) == (int)TicketStatus.Active)
                            str.Add("<span style = 'color: red'> Pending </span>");
                        else if (Convert.ToInt32(d.Status) == (int)TicketStatus.Closed)
                            str.Add("<span style = 'color: green'> Closed </span>");

                        data.Add(str);
                        sl++;
                    }
                    return Json(new
                    {
                        draw = draw,
                        recordsTotal = recordsTotal,
                        recordsFiltered = recordsFiltered,
                        start = start,
                        length = length,
                        data = data
                    });
                }
                catch (Exception ex)
                {
                    var data = new List<object>();
                    return Json(new
                    {
                        draw = draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        start = start,
                        length = length,
                        data = data
                    });
                }
            }
            return Json(HttpNotFound());
        }

    }
}
