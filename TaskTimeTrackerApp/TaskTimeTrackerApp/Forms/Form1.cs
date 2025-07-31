using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using TaskTimeTrackerApp.Models;

namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private List<Project> projects = new List<Project>();
        private List<TaskItem> tasks = new List<TaskItem>();

        private string dataFilePath = "data.json";
        private readonly string dataFile = "projects.json";






        public Form1()
        {
            // Load projects from file
            projects = LoadProjectsFromFile();

            InitializeComponent();

            this.Size = new Size(1200, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Task & Time Tracker";


            this.Load += Form1_Load;
            this.FormClosing += (s, e) => SaveProjectsToFile();

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

            UpdateDashboard();
        }

        private void SaveProjectsToFile()
        {
            try
            {
                foreach (var project in projects)
                {
                    // If tracking, finalize current session before saving
                    if (project.IsTracking && project.StartTime.HasValue)
                    {
                        project.TimeTracked += DateTime.Now - project.StartTime.Value;
                        project.StartTime = DateTime.Now; // keep tracking session active but synced
                    }
                }

                var json = JsonConvert.SerializeObject(projects, Formatting.Indented);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }

        private List<Project> LoadProjectsFromFile()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    var loadedProjects = JsonConvert.DeserializeObject<List<Project>>(json) ?? new List<Project>();

                    foreach (var project in loadedProjects)
                    {
                        // If project was tracking, reset StartTime so new session continues fresh
                        if (project.IsTracking)
                        {
                            project.StartTime = DateTime.Now;
                        }
                    }

                    return loadedProjects;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }

            return new List<Project>();
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            panelSidebar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelSidebar.Width, panelSidebar.Height, 20, 20));
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void ShowPanel(Panel panel)
        {
            panelDashboard.Visible = false;
            panelProjects.Visible = false;
            panelTasks.Visible = false;
            panelReports.Visible = false;
            HistoryPanel.Visible = false;

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
            SetupProjectsPanel();
        }

        private void btnTasks_Click(object sender, EventArgs e)
        {
            ShowPanel(panelTasks);
            SetupTasksPanel();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            ShowPanel(panelReports);
            SetupReportsPanel();
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            ShowPanel(HistoryPanel);
            SetupHistoryPanel(); // ✅ show real history
        }




        private void AddLabelsToPanels()
        {
            AddHeaderLabel(panelDashboard, "Dashboard");
            AddHeaderLabel(panelProjects, "Projects");
            AddHeaderLabel(panelTasks, "Tasks");
            AddHeaderLabel(panelReports, "Reports");
            AddHeaderLabel(HistoryPanel, "History");
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


        private void panelSidebar_Paint(object sender, PaintEventArgs e) { }

    }
}
