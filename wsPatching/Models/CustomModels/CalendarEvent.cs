using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wsPatching.Models.CustomModels
{
    public class CalendarEvent
    {
        public Int64 id { get; set; }

        public String title { get; set; }

        public String start { get; set; }

        public String end { get; set; }

        public bool allDay { get; set; }

        public string color { get; set; }
    }
}
