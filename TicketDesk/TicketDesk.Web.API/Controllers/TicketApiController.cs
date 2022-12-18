using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using TicketDesk.Domain.Model;
using TicketDesk.Web.API.Models;

namespace TicketDesk.Web.API.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api")]
    [Route("{action}")]
    [EnableCors(origins: "http://localhost:58415/", headers: "*", methods: "*")]

    public class TicketApiController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [Route("ticketcategory")]
        public IHttpActionResult GetCategory()
        {
            var result = new Ticket().GetCategoryList();
            //var jsonresult = new JavaScriptSerializer().Serialize(result);
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("tickettype")]
        public IHttpActionResult TicketType()
        {
            var result = new Ticket().GetTicketTypeList();
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ticketpriority")]
        public IHttpActionResult GetPriority()
        {
            var result = new Ticket().GetPriorityList();
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ticketproject")]
        public IHttpActionResult GetProject()
        {
            var result = new Ticket().GetProjectList(0);
            return Ok(result);
        }
    }
}