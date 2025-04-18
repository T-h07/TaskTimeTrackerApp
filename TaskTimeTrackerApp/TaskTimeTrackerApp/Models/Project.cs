using System;
using System.Collections.Generic;

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

        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public string Status { get; set; } = "In Progress";

        public bool IsCompleted { get; set; } = false;



    }
}

