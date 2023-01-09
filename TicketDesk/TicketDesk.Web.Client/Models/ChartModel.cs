using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketDesk.Web.Client.Models
{
    public class Dataset
    {
        public string type { get; set; }
        public string pointBorderColor { get; set; }
        public string pointBackgroundColor { get; set; }
        public bool fill { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public List<int> data { get; set; }
    }

    public class ChartModel
    {
        public ChartModel()
        {
            datasets = new List<Dataset>();
        }
        public List<string> labels { get; set; }
        public List<Dataset> datasets { get; set; }
    }
}