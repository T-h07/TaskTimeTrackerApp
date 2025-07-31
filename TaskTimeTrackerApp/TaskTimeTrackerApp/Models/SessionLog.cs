using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTimeTrackerApp.Models
{
    public class SessionLog
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration => End - Start;
    }

}
