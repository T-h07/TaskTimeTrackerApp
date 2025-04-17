using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTimeTrackerApp.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public DateTime? Deadline { get; set; }
        public TimeSpan TimeTracked { get; set; } = TimeSpan.Zero;
        public bool IsTracking { get; set; }
        public DateTime? StartTime { get; set; }
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
