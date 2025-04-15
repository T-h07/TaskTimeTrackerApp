using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private List<Project> projects = new List<Project>();
        private List<TaskItem> tasks = new List<TaskItem>();

        private Label lblTotalProjects;
        private Label lblTotalTasks;
        private Label lblTasksInProgress;
        private Label lblTimeToday;

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Task & Time Tracker";

            this.Load += Form1_Load;

            ShowPanel(panelDashboard);
            AddLabelsToPanels();

            CreateSummaryBox(panelDashboard, "Total Projects", "0", new Point(30, 120), out lblTotalProjects);
            CreateSummaryBox(panelDashboard, "Total Tasks", "0", new Point(260, 120), out lblTotalTasks);
            CreateSummaryBox(panelDashboard, "Tasks In Progress", "0", new Point(490, 120), out lblTasksInProgress);
            CreateSummaryBox(panelDashboard, "Time Tracked Today", "0h 0m", new Point(720, 120), out lblTimeToday);

            CreateRecentTasksBox(panelDashboard, new Point(30, 240));
            CreatePieChartBox(panelDashboard, new Point(650, 240));
            CreateMotivationAndDeadlines(panelDashboard, new Point(650, 460));
            CreateProgressTracker(panelDashboard, new Point(30, 420));

            LoadSampleData();
            UpdateDashboard();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panelSidebar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelSidebar.Width, panelSidebar.Height, 20, 20));
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse
        );

        private void ShowPanel(Panel panel)
        {
            panelDashboard.Visible = false;
            panelProjects.Visible = false;
            panelTasks.Visible = false;
            panelReports.Visible = false;

            panel.Visible = true;
            panel.BringToFront();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ShowPanel(panelDashboard);
            UpdateDashboard();
        }

        private void btnProjects_Click(object sender, EventArgs e)
        {
            ShowPanel(panelProjects);
        }

        private void btnTasks_Click(object sender, EventArgs e)
        {
            ShowPanel(panelTasks);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            ShowPanel(panelReports);
        }

        private void AddLabelsToPanels()
        {
            AddHeaderLabel(panelDashboard, "Dashboard");
            AddHeaderLabel(panelProjects, "Projects");
            AddHeaderLabel(panelTasks, "Tasks");
            AddHeaderLabel(panelReports, "Reports");
        }

        private void AddHeaderLabel(Panel panel, string text)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                Location = new Point(20, 20),
                AutoSize = true
            };
            panel.Controls.Add(lbl);
        }

        private void CreateSummaryBox(Control parent, string title, string value, Point location, out Label valueLabel)
        {
            Panel box = new Panel
            {
                BackColor = Color.FromArgb(200, 220, 255),
                Size = new Size(200, 80),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(10, 35),
                AutoSize = true
            };

            box.Controls.Add(titleLabel);
            box.Controls.Add(valueLabel);
            parent.Controls.Add(box);
        }

        private void CreateRecentTasksBox(Control parent, Point location)
        {
            Panel box = new Panel
            {
                BackColor = Color.White,
                Size = new Size(600, 150),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = "Recent Tasks",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            ListBox listBox = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(580, 90),
                Font = new Font("Segoe UI", 10)
            };

            var recent = tasks.Take(5).Select(t => $"{t.Title} [{t.Status}]").ToArray();
            listBox.Items.AddRange(recent);

            box.Controls.Add(titleLabel);
            box.Controls.Add(listBox);
            parent.Controls.Add(box);
        }

        private void CreatePieChartBox(Control parent, Point location)
        {
            Chart chart = new Chart
            {
                Size = new Size(300, 200),
                Location = location,
                BackColor = Color.White,
                BorderlineColor = Color.Gray,
                BorderlineDashStyle = ChartDashStyle.Solid,
                BorderlineWidth = 1
            };

            ChartArea chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                ChartType = SeriesChartType.Pie,
                Font = new Font("Segoe UI", 10),
                IsValueShownAsLabel = true
            };

            int inProgress = tasks.Count(t => t.Status == "In Progress");
            int toDo = tasks.Count(t => t.Status == "To Do");
            int done = tasks.Count(t => t.Status == "Done");

            series.Points.AddXY("In Progress", inProgress);
            series.Points.AddXY("To Do", toDo);
            series.Points.AddXY("Done", done);

            chart.Series.Add(series);
            parent.Controls.Add(chart);
        }

        private void CreateMotivationAndDeadlines(Control parent, Point location)
        {
            Panel box = new Panel
            {
                BackColor = Color.White,
                Size = new Size(300, 120),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label quoteLabel = new Label
            {
                Text = "Motivational Quote:\n\"Stay positive, work hard, make it happen.\"",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Location = new Point(10, 10),
                Size = new Size(280, 40)
            };

            Label deadlinesLabel = new Label
            {
                Text = "Upcoming Deadlines:\n• Backend Integration - Apr 20\n• UI Polishing - Apr 22",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(10, 60),
                AutoSize = true
            };

            box.Controls.Add(quoteLabel);
            box.Controls.Add(deadlinesLabel);
            parent.Controls.Add(box);
        }

        private void CreateProgressTracker(Control parent, Point location)
        {
            Panel container = new Panel
            {
                BackColor = Color.White,
                Size = new Size(600, 200),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            Label titleLabel = new Label
            {
                Text = "Progress Tracker",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            container.Controls.Add(titleLabel);

            int yOffset = 40;

            foreach (var task in tasks)
            {
                Label taskLabel = new Label
                {
                    Text = task.Title,
                    Location = new Point(10, yOffset),
                    AutoSize = true
                };

                int value = 0;
                if (task.Status == "In Progress") value = 50;
                else if (task.Status == "Done") value = 100;

                ProgressBar progress = new ProgressBar
                {
                    Location = new Point(10, yOffset + 20),
                    Size = new Size(560, 20),
                    Value = value
                };

                container.Controls.Add(taskLabel);
                container.Controls.Add(progress);

                yOffset += 50;
            }

            parent.Controls.Add(container);
        }

        private void UpdateDashboard()
        {
            lblTotalProjects.Text = projects.Count.ToString();
            lblTotalTasks.Text = tasks.Count.ToString();
            lblTasksInProgress.Text = tasks.Count(t => t.Status == "In Progress").ToString();
            lblTimeToday.Text = "0h 00m";
        }

        private void LoadSampleData()
        {
            projects.Add(new Project { Name = "App UI" });
            projects.Add(new Project { Name = "Backend" });

            tasks.Add(new TaskItem { Title = "Design dashboard", Status = "In Progress" });
            tasks.Add(new TaskItem { Title = "Add login page", Status = "To Do" });
            tasks.Add(new TaskItem { Title = "Database setup", Status = "In Progress" });
            tasks.Add(new TaskItem { Title = "Fix bug #404", Status = "Done" });
            tasks.Add(new TaskItem { Title = "Write docs", Status = "Done" });
        }

        private void panelSidebar_Paint(object sender, PaintEventArgs e) { }
    }

    public class Project
    {
        public string Name { get; set; }
    }

    public class TaskItem
    {
        public string Title { get; set; }
        public string Status { get; set; }
    }
}
