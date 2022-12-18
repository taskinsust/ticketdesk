using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TicketDesk.Domain;
using TicketDesk.Domain.Model;

namespace TicketDesk.Web.API.Models
{
    public static class TicketExtension
    {
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
            //var ss = context.Projects;
            return context.Projects.OrderBy(p => p.ProjectName)
                .ToSelectList(p => p.ProjectId.ToString(), p => p.ProjectName, selectedProject ?? 0, true);

        }

    }
}