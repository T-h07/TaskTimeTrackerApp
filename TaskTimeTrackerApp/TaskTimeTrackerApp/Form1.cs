using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
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

        private Panel panelProjectsList;
        private Button btnAddProject;

        private Dictionary<Project, Timer> projectTimers = new Dictionary<Project, Timer>();
        private Dictionary<Project, Label> projectTimerLabels = new Dictionary<Project, Label>();



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
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse);

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
            SetupProjectsPanel();
        }

        private void btnTasks_Click(object sender, EventArgs e)
        {
            ShowPanel(panelTasks);
            SetupTasksPanel(); // ✅ This was missing!
        }


        private void btnReports_Click(object sender, EventArgs e)
        {
            ShowPanel(panelReports);
            SetupReportsPanel(); // 🟢 This ensures the panel content is built
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

            // ✅ Only include tasks that are NOT Done
            var recent = projects
                .SelectMany(p => p.Tasks.Select(t => new { Project = p.Name, Task = t }))
                .Where(x => x.Task.Status != "Done")
                .OrderByDescending(x => x.Task.Title) // or another property like DateCreated if available
                .Take(5)
                .Select(x => $"{x.Task.Title} [{x.Task.Status}] - {x.Project}")
                .ToArray();

            listBox.Items.AddRange(recent);

            box.Controls.Add(titleLabel);
            box.Controls.Add(listBox);
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

            foreach (var project in projects)
            {
                int total = project.Tasks.Count;
                if (total == 0) continue;

                int done = project.Tasks.Count(t => t.Status == "Done");

                // 🟨 Skip if all tasks are done
                if (done == total) continue;

                int percent = (int)(((double)done / total) * 100);

                Label taskLabel = new Label
                {
                    Text = $"{project.Name} ({done}/{total} tasks completed)",
                    Location = new Point(10, yOffset),
                    AutoSize = true
                };

                ProgressBar progress = new ProgressBar
                {
                    Location = new Point(10, yOffset + 20),
                    Size = new Size(560, 20),
                    Value = percent,
                    ForeColor = Color.Green
                };

                container.Controls.Add(taskLabel);
                container.Controls.Add(progress);

                yOffset += 50;
            }

            parent.Controls.Add(container);
        }




        private void CreatePieChartBox(Control parent, Point location)
        {
            // Create and setup chart
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
                Font = new Font("Segoe UI", 9),
                IsValueShownAsLabel = true, // 👈 show labels directly on slices
                Label = "#PERCENT{P0}",     // 👈 show percentage with no decimals (P0)
                LegendText = "#VALX"        // 👈 this is used in the legend
            };

            var totalProjects = projects.Count;
            var completedProjects = projects.Count(p => p.Tasks.Count > 0 && p.Tasks.All(t => t.Status == "Done"));
            var incompleteProjects = totalProjects - completedProjects;

            if (totalProjects == 0)
            {
                series.Points.AddXY("No Data", 1);
            }
            else
            {
                series.Points.AddXY("Incomplete Projects", incompleteProjects);
                series.Points.AddXY("Completed Projects", completedProjects);
            }

            chart.Series.Add(series);

            // ✅ Add Legend
            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Font = new Font("Segoe UI", 9),
                Alignment = StringAlignment.Center
            };
            chart.Legends.Add(legend);

            parent.Controls.Add(chart);
        }




        private Label lblMotivationalQuote;
        private Label deadlinesLabel;
        private Timer quoteTimer;
        private List<string> motivationalQuotes = new List<string>
{
    "\"Stay positive, work hard, make it happen.\"",
    "\"Success is not final, failure is not fatal: it is the courage to continue that counts.\"",
    "\"Dream big. Start small. Act now.\"",
    "\"Your only limit is your mind.\"",
    "\"Push yourself, because no one else is going to do it for you.\""
};
        private int currentQuoteIndex = 0;

        private void CreateMotivationAndDeadlines(Control parent, Point location)
        {
            Panel box = new Panel
            {
                BackColor = Color.White,
                Size = new Size(300, 200),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblMotivationalQuote = new Label
            {
                Text = "Motivational Quote:\n" + motivationalQuotes[currentQuoteIndex],
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Location = new Point(10, 10),
                MaximumSize = new Size(280, 0),
                AutoSize = true
            };

            box.Controls.Add(lblMotivationalQuote);

            // Delay deadline positioning until label size is resolved
            deadlinesLabel = new Label
            {
                Font = new Font("Segoe UI", 10),
                MaximumSize = new Size(280, 0),
                AutoSize = true
            };

            box.Controls.Add(deadlinesLabel);
            parent.Controls.Add(box);

            UpdateDeadlinesLabel();
            StartMotivationalQuoteTimer(); // 💡 Ensure it always runs
        }

        private void UpdateDeadlinesLabel()
        {
            if (deadlinesLabel == null) return;

            string deadlinesText = string.Join("\n", projects
                .Where(p => p.Deadline != null)
                .OrderBy(p => p.Deadline)
                .Take(3)
                .Select(p =>
                {
                    int total = p.Tasks.Count;
                    int done = p.Tasks.Count(t => t.Status == "Done");
                    string status = total == 0
                        ? "No tasks"
                        : (done == total ? "✅ Completed" : $"{total - done} remaining");
                    return $"• {p.Name} - {p.Deadline?.ToShortDateString()} ({status})";
                }));

            deadlinesLabel.Text = "Upcoming Deadlines:\n" + (string.IsNullOrEmpty(deadlinesText) ? "No deadlines." : deadlinesText);
            deadlinesLabel.Location = new Point(10, lblMotivationalQuote.Bottom + 10);
        }

        private void StartMotivationalQuoteTimer()
        {
            if (quoteTimer != null)
            {
                quoteTimer.Stop();
                quoteTimer.Dispose();
            }

            quoteTimer = new Timer();
            quoteTimer.Interval = 10000; // 10 seconds
            quoteTimer.Tick += (s, e) =>
            {
                currentQuoteIndex = (currentQuoteIndex + 1) % motivationalQuotes.Count;
                if (lblMotivationalQuote != null)
                {
                    lblMotivationalQuote.Text = "Motivational Quote:\n" + motivationalQuotes[currentQuoteIndex];
                    UpdateDeadlinesLabel(); // Ensure repositioning on quote change
                }
            };
            quoteTimer.Start();
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

        private void SetupProjectsPanel()
        {
            int leftMargin = 40;
            int buttonY = 70;
            int listY = buttonY + 50;

            if (btnAddProject == null)
            {
                btnAddProject = new Button
                {
                    Text = "➕ New Project",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Size = new Size(150, 40),
                    Location = new Point(leftMargin, buttonY),
                    BackColor = Color.LightSkyBlue
                };
                btnAddProject.Click += BtnAddProject_Click;
                panelProjects.Controls.Add(btnAddProject);
            }

            if (panelProjectsList == null)
            {
                panelProjectsList = new Panel
                {
                    Location = new Point(leftMargin, listY),
                    Size = new Size(panelProjects.Width - leftMargin - 20, panelProjects.Height - listY),
                    AutoScroll = true
                };
                panelProjects.Controls.Add(panelProjectsList);
            }

            LoadProjectsIntoPanel();
        }


        private void LoadProjectsIntoPanel()
        {
            panelProjectsList.Controls.Clear();
            int cardWidth = 600;
            int yOffset = 0;

            foreach (var project in projects)
            {
                int baseHeight = 280;

                Panel card = new Panel
                {
                    Size = new Size(cardWidth, baseHeight),
                    Location = new Point(0, yOffset),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblTitle = new Label
                {
                    Text = project.Name,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                Label lblTimer = new Label
                {
                    Text = $"Time: {project.TimeTracked:hh\\:mm\\:ss}",
                    Location = new Point(10, 40),
                    AutoSize = true
                };
                projectTimerLabels[project] = lblTimer;

                Label lblDesc = new Label
                {
                    Text = project.Description,
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    Location = new Point(10, 65),
                    Size = new Size(570, 40),
                    MaximumSize = new Size(570, 0),
                    AutoSize = true
                };

                Label lblDeadline = new Label
                {
                    Text = $"Deadline: {project.Deadline?.ToShortDateString()}",
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    Location = new Point(10, 90),
                    AutoSize = true
                };

                Button btnStartStop = new Button
                {
                    Text = project.IsTracking ? "Stop" : "Start",
                    Location = new Point(150, 35),
                    Size = new Size(80, 25)
                };

                btnStartStop.Click += (s, e) =>
                {
                    if (!projectTimers.ContainsKey(project))
                    {
                        projectTimers[project] = new Timer { Interval = 1000 };
                        projectTimers[project].Tick += (sender, args) =>
                        {
                            if (project.IsTracking && project.StartTime.HasValue)
                            {
                                project.TimeTracked = DateTime.Now - project.StartTime.Value;
                                if (projectTimerLabels.ContainsKey(project))
                                {
                                    projectTimerLabels[project].Text = $"Time: {project.TimeTracked:hh\\:mm\\:ss}";
                                }
                                UpdateDashboard();
                            }
                        };
                    }

                    if (project.IsTracking)
                    {
                        project.IsTracking = false;
                        if (project.StartTime.HasValue)
                            project.TimeTracked = DateTime.Now - project.StartTime.Value;
                        project.StartTime = null;
                        projectTimers[project].Stop();
                    }
                    else
                    {
                        project.IsTracking = true;
                        project.StartTime = DateTime.Now - project.TimeTracked;
                        projectTimers[project].Start();
                    }

                    LoadProjectsIntoPanel();
                };

                Button btnAddTask = new Button
                {
                    Text = "Add Task",
                    Location = new Point(250, 35),
                    Size = new Size(80, 25)
                };

                btnAddTask.Click += (s, e) =>
                {
                    using (var inputDialog = new InputDialog("New Task", $"Enter task title for {project.Name}:"))
                    {
                        if (inputDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputDialog.UserInput))
                        {
                            project.Tasks.Add(new TaskItem
                            {
                                Title = inputDialog.UserInput,
                                Description = inputDialog.DescriptionInput // ✅ Uses description field
                            });
                            LoadProjectsIntoPanel();
                            UpdateDashboard();
                        }
                    }
                };

                Button btnRemoveProject = new Button
                {
                    Text = "Remove",
                    Location = new Point(340, 35),
                    Size = new Size(80, 25),
                    BackColor = Color.LightCoral
                };

                btnRemoveProject.Click += (s, e) =>
                {
                    var result = MessageBox.Show($"Are you sure you want to delete '{project.Name}'?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        if (projectTimers.ContainsKey(project))
                        {
                            projectTimers[project].Stop();
                            projectTimers.Remove(project);
                        }

                        projects.Remove(project);
                        LoadProjectsIntoPanel();
                        UpdateDashboard();
                    }
                };

                // Scrollable container for tasks
                Panel taskContainer = new Panel
                {
                    Location = new Point(10, 120),
                    Size = new Size(cardWidth - 20, 140),
                    AutoScroll = true,
                    BorderStyle = BorderStyle.None
                };

                int taskYOffset = 0;
                foreach (var task in project.Tasks)
                {
                    Label lblTaskTitle = new Label
                    {
                        Text = task.Title,
                        Location = new Point(0, taskYOffset),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold)
                    };

                    Label lblTaskDesc = new Label
                    {
                        Text = string.IsNullOrWhiteSpace(task.Description) ? "(No Description)" : task.Description,
                        Location = new Point(0, taskYOffset + 18),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9, FontStyle.Italic),
                        ForeColor = Color.Gray
                    };

                    ComboBox cmbStatus = new ComboBox
                    {
                        Location = new Point(190, taskYOffset),
                        Size = new Size(100, 25),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    cmbStatus.Items.AddRange(new[] { "To Do", "In Progress", "Done" });
                    cmbStatus.SelectedItem = task.Status;
                    cmbStatus.SelectedIndexChanged += (s, e) =>
                    {
                        task.Status = cmbStatus.SelectedItem.ToString();
                        UpdateDashboard();
                    };

                    Button btnRemoveTask = new Button
                    {
                        Text = "🗑",
                        Size = new Size(30, 25),
                        Location = new Point(300, taskYOffset),
                        BackColor = Color.LightCoral,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnRemoveTask.FlatAppearance.BorderSize = 0;
                    btnRemoveTask.Click += (s, e) =>
                    {
                        project.Tasks.Remove(task);
                        LoadProjectsIntoPanel();
                        UpdateDashboard();
                    };

                    taskContainer.Controls.Add(lblTaskTitle);
                    taskContainer.Controls.Add(lblTaskDesc);
                    taskContainer.Controls.Add(cmbStatus);
                    taskContainer.Controls.Add(btnRemoveTask);

                    taskYOffset += 50;
                }

                // Final layout assembly
                card.Controls.Add(lblTitle);
                card.Controls.Add(lblTimer);
                card.Controls.Add(lblDesc);
                card.Controls.Add(lblDeadline);
                card.Controls.Add(btnStartStop);
                card.Controls.Add(btnAddTask);
                card.Controls.Add(btnRemoveProject);
                card.Controls.Add(taskContainer);

                panelProjectsList.Controls.Add(card);
                yOffset += card.Height + 15;
            }
        }


        // Add this inside your Form1.cs

        private ComboBox cmbReportProjects;
        private RichTextBox txtProjectReportDesc;
        private Panel panelReportTaskBoxes;
        private Label lblReportTime;
        private Label lblReportDays;
        private ProgressBar reportProgressBar;
        private Button btnDownloadReport;

        private void SetupReportsPanel()
        {
            panelReports.Controls.Clear();

            Label lblHeader = new Label
            {
                Text = "Project Report",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelReports.Controls.Add(lblHeader);

            cmbReportProjects = new ComboBox
            {
                Location = new Point(20, 60),
                Size = new Size(400, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportProjects.Items.AddRange(projects.Select(p => p.Name).ToArray());
            if (cmbReportProjects.Items.Count > 0)
                cmbReportProjects.SelectedIndex = 0;
            cmbReportProjects.SelectedIndexChanged += (s, e) => UpdateReportPanel();
            panelReports.Controls.Add(cmbReportProjects);

            txtProjectReportDesc = new RichTextBox
            {
                Location = new Point(20, 100),
                Size = new Size(700, 100),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelReports.Controls.Add(txtProjectReportDesc);

            lblReportTime = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 210),
                AutoSize = true
            };
            panelReports.Controls.Add(lblReportTime);

            lblReportDays = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(300, 210),
                AutoSize = true
            };
            panelReports.Controls.Add(lblReportDays);

            reportProgressBar = new ProgressBar
            {
                Location = new Point(20, 240),
                Size = new Size(500, 20)
            };
            panelReports.Controls.Add(reportProgressBar);

            panelReportTaskBoxes = new Panel
            {
                Location = new Point(20, 270),
                Size = new Size(panelReports.Width - 40, panelReports.Height - 330),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelReports.Controls.Add(panelReportTaskBoxes);

            btnDownloadReport = new Button
            {
                Text = "Download PDF",
                Location = new Point(600, 60),
                Size = new Size(120, 30),
                BackColor = Color.LightGreen
            };
            btnDownloadReport.Click += BtnDownloadReport_Click;
            panelReports.Controls.Add(btnDownloadReport);

            UpdateReportPanel();
        }

        private void UpdateReportPanel()
        {
            if (cmbReportProjects.SelectedItem == null) return;

            string selectedProjectName = cmbReportProjects.SelectedItem.ToString();
            var project = projects.FirstOrDefault(p => p.Name == selectedProjectName);
            if (project == null) return;

            txtProjectReportDesc.Text = project.Description;

            lblReportTime.Text = $"Time Tracked: {project.TimeTracked:hh\\:mm\\:ss}";
            lblReportDays.Text = $"~{(int)Math.Ceiling(project.TimeTracked.TotalDays)} day(s)";

            int total = project.Tasks.Count;
            int done = project.Tasks.Count(t => t.Status == "Done");
            reportProgressBar.Value = total > 0 ? (int)(((double)done / total) * 100) : 0;

            panelReportTaskBoxes.Controls.Clear();
            int yOffset = 10;

            foreach (var task in project.Tasks)
            {
                Panel taskBox = new Panel
                {
                    Location = new Point(10, yOffset),
                    Size = new Size(panelReportTaskBoxes.Width - 30, 70),
                    BackColor = Color.WhiteSmoke,
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblTitle = new Label
                {
                    Text = $"{task.Title} [{task.Status}]",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(10, 5),
                    AutoSize = true
                };

                Label lblDesc = new Label
                {
                    Text = task.Description,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(10, 25),
                    AutoSize = true
                };

                taskBox.Controls.Add(lblTitle);
                taskBox.Controls.Add(lblDesc);
                panelReportTaskBoxes.Controls.Add(taskBox);
                yOffset += 80;
            }
        }

        private void BtnDownloadReport_Click(object sender, EventArgs e)
        {
            if (cmbReportProjects.SelectedItem == null) return;

            string selectedProjectName = cmbReportProjects.SelectedItem.ToString();
            var project = projects.FirstOrDefault(p => p.Name == selectedProjectName);
            if (project == null) return;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save PDF Report";
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.FileName = $"{project.Name}_Report.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var doc = new iTextSharp.text.Document())
                    {
                        iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                        doc.Open();

                        doc.Add(new iTextSharp.text.Paragraph("Project Report"));
                        doc.Add(new iTextSharp.text.Paragraph("\n"));
                        doc.Add(new iTextSharp.text.Paragraph($"Project Name: {project.Name}"));
                        doc.Add(new iTextSharp.text.Paragraph($"Description: {project.Description}"));
                        doc.Add(new iTextSharp.text.Paragraph($"Time Tracked: {project.TimeTracked}"));
                        doc.Add(new iTextSharp.text.Paragraph($"Estimated Days: {(int)Math.Ceiling(project.TimeTracked.TotalDays)} day(s)"));
                        doc.Add(new iTextSharp.text.Paragraph("\nTasks:"));

                        foreach (var task in project.Tasks)
                        {
                            doc.Add(new iTextSharp.text.Paragraph($"- {task.Title} [{task.Status}] - {task.Description}"));
                        }

                        doc.Close();
                    }

                    MessageBox.Show($"Report saved to:\n{filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }







        private void BtnAddProject_Click(object sender, EventArgs e)
        {
            using (var dialog = new ProjectDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.ProjectName))
                {
                    projects.Add(new Project
                    {
                        Name = dialog.ProjectName,
                        Description = dialog.ProjectDescription,
                        Deadline = dialog.SelectedDeadline
                    });
                    SetupProjectsPanel(); // Also ensures it's initialized
                    UpdateDashboard();
                }
            }
        }

        private void UpdateDashboard()
        {
            lblTotalProjects.Text = projects.Count.ToString();
            lblTotalTasks.Text = projects.Sum(p => p.Tasks.Count).ToString();
            lblTasksInProgress.Text = projects.Sum(p => p.Tasks.Count(t => t.Status == "In Progress")).ToString();
            lblTimeToday.Text = $"{projects.Sum(p => p.TimeTracked.TotalHours):0}h {projects.Sum(p => p.TimeTracked.Minutes):00}m";

            // 🧼 Clear and re-add the Motivation Box
            panelDashboard.Controls.OfType<Panel>()
                .Where(p => p.Controls.OfType<Label>().Any(l => l.Text.StartsWith("Motivational Quote")))
                .ToList()
                .ForEach(p => panelDashboard.Controls.Remove(p));
            CreateMotivationAndDeadlines(panelDashboard, new Point(650, 460));

            // 🧼 Clear and re-add the Recent Tasks Box
            panelDashboard.Controls.OfType<Panel>()
                .Where(p => p.Controls.OfType<Label>().Any(l => l.Text == "Recent Tasks"))
                .ToList()
                .ForEach(p => panelDashboard.Controls.Remove(p));
            CreateRecentTasksBox(panelDashboard, new Point(30, 240));

            // 🧼 Clear and re-add the Pie Chart (project completion)
            panelDashboard.Controls.OfType<Chart>()
                .ToList()
                .ForEach(c => panelDashboard.Controls.Remove(c));
            CreatePieChartBox(panelDashboard, new Point(650, 240));

            // 🧼 Clear and re-add Progress Tracker if needed
            panelDashboard.Controls.OfType<Panel>()
                .Where(p => p.Controls.OfType<Label>().Any(l => l.Text == "Progress Tracker"))
                .ToList()
                .ForEach(p => panelDashboard.Controls.Remove(p));
            CreateProgressTracker(panelDashboard, new Point(30, 420));
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


        private ComboBox cmbProjectFilter;
        private ComboBox cmbStatusFilter;
        private TextBox txtSearch = new TextBox(); // ← make sure this is initialized!
        private Panel panelTaskList;
        private Button btnAddTask;


        private void SetupTasksPanel()
        {
            panelTasks.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "All Tasks",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelTasks.Controls.Add(lblTitle);

            txtSearch = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(200, 25),
                ForeColor = Color.Gray,
                Text = "Search tasks..."
            };

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "Search tasks...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };

            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Search tasks...";
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            panelTasks.Controls.Add(txtSearch);

            cmbProjectFilter = new ComboBox
            {
                Location = new Point(240, 60),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProjectFilter.Items.Clear();
            cmbProjectFilter.Items.Add("All Projects");
            cmbProjectFilter.Items.AddRange(projects.Select(p => p.Name).ToArray());
            cmbProjectFilter.SelectedIndex = 0;
            cmbProjectFilter.SelectedIndexChanged += (s, e) => RefreshTaskList();
            panelTasks.Controls.Add(cmbProjectFilter);

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(460, 60),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatusFilter.Items.Clear();
            cmbStatusFilter.Items.AddRange(new[] { "All", "To Do", "In Progress", "Done" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => RefreshTaskList();
            panelTasks.Controls.Add(cmbStatusFilter);

            btnAddTask = new Button
            {
                Text = "➕ Add Task",
                Location = new Point(630, 60),
                Size = new Size(120, 25),
                BackColor = Color.LightGreen
            };
            btnAddTask.Click += BtnAddTask_Click;
            panelTasks.Controls.Add(btnAddTask);

            panelTaskList = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(panelTasks.Width - 40, panelTasks.Height - 130),
                AutoScroll = true
            };
            panelTasks.Controls.Add(panelTaskList);

            RefreshTaskList(); // 🟢 Final refresh for task listing
        }


        private void RefreshTaskList()
        {
            panelTaskList.Controls.Clear();

            string searchText = txtSearch.Text.ToLower();
            if (searchText == "search tasks...") searchText = "";

            string selectedProject = cmbProjectFilter.SelectedItem?.ToString();
            string selectedStatus = cmbStatusFilter.SelectedItem?.ToString();

            var filteredTasks = new List<(Project Project, TaskItem Task)>();

            foreach (var project in projects)
            {
                foreach (var task in project.Tasks)
                {
                    bool matchesProject = selectedProject == "All Projects" || project.Name == selectedProject;
                    bool matchesStatus = selectedStatus == "All" || task.Status == selectedStatus;
                    bool matchesSearch = string.IsNullOrWhiteSpace(searchText) || task.Title.ToLower().Contains(searchText);

                    if (matchesProject && matchesStatus && matchesSearch)
                    {
                        filteredTasks.Add((project, task));
                    }
                }
            }

            int yOffset = 10;
            foreach (var (project, task) in filteredTasks)
            {
                Label lblTitle = new Label
                {
                    Text = $"[{project.Name}] {task.Title}",
                    Location = new Point(10, yOffset),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true
                };

                Label lblDesc = new Label
                {
                    Text = task.Description,
                    Location = new Point(10, yOffset + 20),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true
                };

                ComboBox cmbStatus = new ComboBox
                {
                    Location = new Point(320, yOffset),
                    Size = new Size(100, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbStatus.Items.AddRange(new[] { "To Do", "In Progress", "Done" });
                cmbStatus.SelectedItem = task.Status;
                cmbStatus.SelectedIndexChanged += (s, e) =>
                {
                    task.Status = cmbStatus.SelectedItem.ToString();
                    UpdateDashboard();
                };

                Button btnRemove = new Button
                {
                    Text = "🗑️",
                    Location = new Point(430, yOffset),
                    Size = new Size(30, 25),
                    BackColor = Color.LightCoral
                };
                btnRemove.Click += (s, e) =>
                {
                    project.Tasks.Remove(task);
                    RefreshTaskList();
                    LoadProjectsIntoPanel();
                    UpdateDashboard();
                };

                panelTaskList.Controls.Add(lblTitle);
                panelTaskList.Controls.Add(lblDesc);
                panelTaskList.Controls.Add(cmbStatus);
                panelTaskList.Controls.Add(btnRemove);

                yOffset += 60;
            }
        }

        // Add this inside your Form1.cs or main form code
        



        private void BtnAddTask_Click(object sender, EventArgs e)
        {
            var dialog = new InputDialog("New Task", "Enter task title:");
            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.UserInput))
            {
                var projectSelector = new Form
                {
                    Text = "Select Project",
                    Size = new Size(300, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog
                };

                ComboBox projectBox = new ComboBox
                {
                    Location = new Point(30, 20),
                    Size = new Size(220, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                projectBox.Items.AddRange(projects.Select(p => p.Name).ToArray());
                if (projectBox.Items.Count > 0)
                    projectBox.SelectedIndex = 0;

                Button ok = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(90, 60)
                };

                projectSelector.Controls.Add(projectBox);
                projectSelector.Controls.Add(ok);
                projectSelector.AcceptButton = ok;

                if (projectSelector.ShowDialog() == DialogResult.OK)
                {
                    var selectedProject = projects.FirstOrDefault(p => p.Name == projectBox.SelectedItem?.ToString());
                    if (selectedProject != null)
                    {
                        selectedProject.Tasks.Add(new TaskItem
                        {
                            Title = dialog.UserInput, // ✅ set title correctly
                            Description = dialog.DescriptionInput // ✅ set description
                        });

                        SetupProjectsPanel();
                        SetupTasksPanel();
                        UpdateDashboard();
                    }
                }
            }
        }





    }








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

    public class TaskItem
    {
        public string Title { get; set; }
        public string Status { get; set; } = "To Do";
        public string Description { get; set; } = "";
    }


}