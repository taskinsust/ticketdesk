using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketDesk.Domain.Model.VM
{
    public class TopDelayedTaskVM
    {
        public string TicketId { get; set; }
        public string TicketTitle { get; set; }
        public string CompletionTime { get; set; }

        public string TicketOwner { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string TicketCLosedBy { get; set; }
        public DateTimeOffset TicketCLosingDate { get; set; }
        public string Status { get; set; }
    }
}
