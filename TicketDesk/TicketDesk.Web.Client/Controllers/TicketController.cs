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
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TicketDesk.Domain;
using TicketDesk.Domain.Model;
using TicketDesk.Domain.Model.Extensions;
using TicketDesk.IO;
using TicketDesk.Localization.Controllers;
using TicketDesk.Web.Identity;

namespace TicketDesk.Web.Client.Controllers
{
    /// <summary>
    /// Class TicketController.
    /// </summary>
    [TdAuthorize]
    [RoutePrefix("ticket")]
    [Route("{action=index}")]
    //[TdAuthorize(Roles = "TdInternalUsers,TdHelpDeskUsers,TdAdministrators")]
    public class TicketController : Controller
    {
        private TdDomainContext Context { get; set; }
        private TicketDeskUserManager UserManager { get; set; }
        public TicketController(TdDomainContext context, TicketDeskUserManager userManager)
        {
            Context = context;
            UserManager = userManager;
        }

        public RedirectToRouteResult Index()
        {
            return RedirectToAction("Index", "TicketCenter");
        }

        [Route("{id:int}")]
        public async Task<ActionResult> Index(int id)
        {

            var model = await Context.Tickets.Include(t => t.TicketSubscribers).FirstOrDefaultAsync(t => t.TicketId == id);
            if (model == null)
            {
                return RedirectToAction("Index", "TicketCenter");
            }
            ViewBag.IsEditorDefaultHtml = Context.TicketDeskSettings.ClientSettings.GetDefaultTextEditorType() == "summernote";

            return View(model);
        }

        [Route("new")]
        public async Task<ActionResult> New()
        {
            var model = new Ticket
            {
                Owner = Context.SecurityProvider.CurrentUserId,
                IsHtml = Context.TicketDeskSettings.ClientSettings.GetDefaultTextEditorType() == "summernote"
            };

            await SetProjectInfoForModelAsync(model);

            ViewBag.TempId = Guid.NewGuid();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateOnlyIncomingValues]
        [ValidateInput(false)]
        [Route("new")]
        public async Task<ActionResult> New(Ticket ticket, Guid tempId)
        {
            if (ticket.IsHtml)
            {
                ticket.Details = ticket.Details.StripHtmlWhenEmpty();
                if (string.IsNullOrEmpty(ticket.Details))
                {
                    ModelState.AddModelError("Details", Strings.RequiredField);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await CreateTicketAsync(ticket, tempId))
                    {
                        return RedirectToAction("Index", new { id = ticket.TicketId });
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (DbEntityValidationException)
                {

                    //TODO: catch rule exceptions? or can annotations handle this fully now?
                }

            }
            ViewBag.TempId = tempId;
            await SetProjectInfoForModelAsync(ticket);
            return View(ticket);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("apinew")]
        public async Task<JsonResult> ApiNew(TicketEntity ticketEntity)
        {
            try
            {
                Ticket ticket = new Ticket
                {
                    TicketType = "Problem",
                    Category = "Other",
                    Title = ticketEntity.Title,
                    CreatedBy = UserManager.Users.Where(x => x.TelegramUserId == ticketEntity.TelegramUserId).SingleOrDefault().Id,
                    Owner = UserManager.Users.Where(x => x.TelegramUserId == ticketEntity.TelegramUserId).SingleOrDefault().Id,
                    CreatedDate = DateTime.Now,
                    TicketStatus = TicketStatus.Active,
                    ProjectId = 1,// Default project
                    Details = ticketEntity.Title,
                    LastUpdateDate = DateTime.Now,
                    CurrentStatusDate = DateTime.Now,

                };

                try
                {

                    var duplicateTicket = Context.Tickets.Where(x => x.Title == ticket.Title && x.Owner == ticket.Owner 
                    && x.CreatedDate.DateTime == DateTime.Today).ToList().Count;

                    if (duplicateTicket > 1)
                        return Json(new { IsSuccess = true, Message = "success" });

                }
                catch (Exception e)
                {
                    throw e;
                }

                SqlConnection con;
                SqlCommand cmd;
                SqlDataAdapter da;
                string sql = string.Format(@"INSERT INTO  [Tickets]
                                   ([TicketType]
                                   ,[Category]
                                   ,[Title]
                                   ,[Details]
                                  
                                   ,[CreatedBy]
                                   ,[CreatedDate]
                                   ,[Owner]
                                   ,[TicketStatus]
                                   
                                   ,[CurrentStatusDate]
                                   ,[CurrentStatusSetBy]
                                   ,[LastUpdateBy]
                                   ,[LastUpdateDate]
                                   
                                   ,[ProjectId]
                                   ,IsHtml, AffectsCustomer
                                  )
                                  VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', {7}, '{8}', '{9}', '{10}', '{11}', {12}, {13}, {14})",
                                            ticket.TicketType,
                                            ticket.Category,
                                            ticket.Title,
                                            ticket.Title,

                                            ticket.Owner,
                                            DateTime.Now,
                                            ticket.Owner,
                                            0,

                                            DateTime.Now,
                                            ticket.Owner,
                                            ticket.Owner,
                                            DateTime.Now,

                                            ticket.ProjectId,
                                            1, 1
                                           );
                using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["TicketDesk"].ToString()))
                {
                    try
                    {
                        con.Open();
                        cmd = new SqlCommand(sql, con);
                        //cmd.CommandTimeout = 250; //seconds
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {

                    }
                }

                return Json(new { IsSuccess = true, Message = "success" });
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (DbEntityValidationException)
            {
                return Json(new { IsSuccess = false, Message = "failed" });
                //TODO: catch rule exceptions? or can annotations handle this fully now?
            }
        }

        protected String AddSlashes(String strData)
        {
            if (String.IsNullOrEmpty(strData))
                return "";
            //adding slashes for database
            strData = strData.Replace("\\", "\\\\");    //add a slash before   slash    character
            strData = strData.Replace("'", "\\'");      //add a slash before   single   quote character
            strData = strData.Replace("\"", "\\\"");    //add a slash before   double   quote character
            return strData;
        }

        [Route("ticket-files")]
        public ActionResult TicketFiles(int ticketId)
        {
            //WARNING! This is also used as a child action and cannot be made async in MVC 5
            var attachments = TicketDeskFileStore.ListAttachmentInfo(ticketId.ToString(CultureInfo.InvariantCulture), false);
            ViewBag.TicketId = ticketId;
            return PartialView("_TicketFiles", attachments);
        }

        [Route("ticket-events")]
        public ActionResult TicketEvents(int ticketId)
        {
            //WARNING! This is also used as a child action and cannot be made async in MVC 5
            var ticket = Context.Tickets.Find(ticketId);
            return PartialView("_TicketEvents", ticket.TicketEvents);
        }

        [Route("ticket-details")]
        public ActionResult TicketDetails(int ticketId)
        {
            //WARNING! This is also used as a child action and cannot be made async in MVC 5
            var ticket = Context.Tickets.Find(ticketId);
            ViewBag.DisplayProjects = Context.Projects.Any();

            return PartialView("_TicketDetails", ticket);
        }

        [Route("change-ticket-subscription")]
        [HttpPost]
        public async Task<JsonResult> ChangeTicketSubscription(int ticketId)
        {
            var userId = Context.SecurityProvider.CurrentUserId;
            var ticket = await Context.Tickets.Include(t => t.TicketSubscribers).Include(t => t.TicketEvents.Select(e => e.TicketEventNotifications)).FirstOrDefaultAsync(t => t.TicketId == ticketId);
            var subscriber =
                ticket.TicketSubscribers.FirstOrDefault(s => s.SubscriberId == Context.SecurityProvider.CurrentUserId);
            var isSubscribed = false;
            if (subscriber == null)
            {
                subscriber = new TicketSubscriber
                {
                    SubscriberId = userId,
                };
                ticket.TicketSubscribers.Add(subscriber);
                isSubscribed = true;
            }
            else
            {
                ticket.TicketSubscribers.Remove(subscriber);
            }
            await Context.SaveChangesAsync();
            return new JsonCamelCaseResult { Data = new { IsSubscribed = isSubscribed } };
        }

        private async Task SetProjectInfoForModelAsync(Ticket ticket)
        {
            if (ticket.ProjectId == default(int))
            {
                var projects = await Context.Projects.Select(s => s.ProjectId).ToListAsync();
                var isMulti = (projects.Count > 1);
                ViewBag.IsMultiProject = isMulti;

                //set to first project if only one project exists, otherwise use user's selected project
                ticket.ProjectId = (isMulti) ? await Context.UserSettingsManager.GetUserSelectedProjectIdAsync(Context) : projects.FirstOrDefault();
            }
        }


        private async Task<bool> CreateTicketAsync(Ticket ticket, Guid tempId)
        {

            Context.Tickets.Add(ticket);
            await Context.SaveChangesAsync();
            ticket.CommitPendingAttachments(tempId);

            return ticket.TicketId != default(int);
        }

    }
}