using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketDesk.Domain.Model.Extensions
{
    public class TicketEntity
    {
        public TicketEntity() { }
        public string TicketType { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string CreatedBy { get; set; }
        public string Owner { get; set; }
        public int TicketStatus { get; set; } = 1;
        public string LastUpdateBy { get; set; }
        public string CurrentStatusSetBy { get; set; }

        public string TelegramUserId { get; set; }
        public string FirstName { get; set; }

    }
}
