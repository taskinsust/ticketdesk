using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketDesk.Domain.Model.VM
{
    public class AvgClosingTimeVM
    {
        public int TotalClosed { get; set; }
        public DateTime ClosingDate { get; set; }
        public int AVGHour { get; set; }
    }
}
