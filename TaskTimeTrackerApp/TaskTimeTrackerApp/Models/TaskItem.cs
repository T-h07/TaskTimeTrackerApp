using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTimeTrackerApp.Models
{
    public class TaskItem
    {
        public string Title { get; set; }
        public string Status { get; set; } = "To Do";
        public string Description { get; set; } = "";
    }
}