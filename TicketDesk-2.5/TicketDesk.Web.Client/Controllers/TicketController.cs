﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Antlr.Runtime;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using TicketDesk.Domain;
using TicketDesk.Domain.Model;
using TicketDesk.Web.Client.Models;

namespace TicketDesk.Web.Client.Controllers
{
    [RoutePrefix("ticket")]
    [Authorize]
    public class TicketController : Controller
    {
        private TicketDeskContext Context { get; set; }
        public TicketController(TicketDeskContext context)
        {
            Context = context;
        }

        public RedirectToRouteResult Index()
        {
            return RedirectToAction("Index", "TicketCenter", new { area = "" });
        }

        [Route("{id:int}")]
        public async Task<ActionResult> Index(int id)
        {
            var model = await Context.Tickets.FindAsync(id);
            return View(model);
        }

        [Authorize(Roles = "TdInternalUsers")]
        public ActionResult New()
        {
            var model = new TicketCreateViewModel(new Ticket(), Context);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> New(Ticket ticket, Guid tempId)
        {

            //TODO: Fix labels in view to use @Html.LabelFor
            //TODO: What the fuck is up with the taglist not rebining correctly?
            //TODO: Model validation attributes for include/exclude properties
            var vm = new TicketCreateViewModel(ticket, Context);
            try
            {
                if (await vm.CreateTicketAsync())
                {
                    return RedirectToAction("Index", new {id = ticket.TicketId});
                }
            }
            catch (DbEntityValidationException ex)//TODO: catch these and add to modelstate
            {
                var d =ex;
            }
            //TODO: catch rule exceptions? or can annotations handle this fully now?

            return View(new TicketCreateViewModel(ticket, Context));
        }
    }
}