using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaskTimeTrackerApp.Models;
using Newtonsoft.Json;

namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private Panel panelProjectsList;
        private Button btnAddProject;
        private Dictionary<Project, Timer> projectTimers = new Dictionary<Project, Timer>();
        private Dictionary<Project, Label> projectTimerLabels = new Dictionary<Project, Label>();

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

            foreach (var project in projects.Where(p => !p.IsCompleted))
            {
                int baseHeight = 280;

                Panel card = new Panel
                {
                    Size = new Size(cardWidth, baseHeight),
                    Location = new Point(0, yOffset),
                    BackColor = project.Deadline.HasValue && project.Deadline < DateTime.Now ? Color.MistyRose : Color.White, // Highlight overdue
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
                    Font = new Font("Segoe UI", 9),
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
                    if (project.IsTracking)
                    {
                        // STOP
                        if (project.StartTime.HasValue)
                        {
                            var end = DateTime.Now;
                            var duration = end - project.StartTime.Value;
                            project.TimeTracked += duration;

                            // Only log if longer than 30 seconds
                            if (duration.TotalSeconds > 30)
                            {
                                project.SessionHistory.Add(new SessionLog
                                {
                                    Start = project.StartTime.Value,
                                    End = end
                                });
                            }
                        }
                        project.IsTracking = false;
                        project.StartTime = null;
                        if (projectTimers.ContainsKey(project))
                            projectTimers[project].Stop();
                    }

                    else
                    {
                        // START
                        project.IsTracking = true;
                        project.StartTime = DateTime.Now;

                        if (!projectTimers.ContainsKey(project))
                        {
                            projectTimers[project] = new Timer { Interval = 1000 };
                            projectTimers[project].Tick += (sender, args) =>
                            {
                                if (project.IsTracking && project.StartTime.HasValue)
                                {
                                    var liveTime = project.TimeTracked + (DateTime.Now - project.StartTime.Value);
                                    projectTimerLabels[project].Text = $"Time: {liveTime:hh\\:mm\\:ss}";
                                    UpdateDashboard();
                                }
                            };
                        }
                        projectTimers[project].Start();

                        projectTimerLabels[project].Text = $"Time: {project.TimeTracked:hh\\:mm\\:ss}";
                        btnStartStop.Text = "Stop";  // <-- Update text here
                    }

                    SaveProjectsToFile();
                    UpdateDashboard();
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
                        if (inputDialog.ShowDialog() == DialogResult.OK)
                        {
                            string taskName = inputDialog.UserInput?.Trim();
                            if (string.IsNullOrWhiteSpace(taskName))
                            {
                                MessageBox.Show("Task name cannot be empty.", "Invalid Task", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            project.Tasks.Add(new TaskItem
                            {
                                Title = taskName,
                                Description = inputDialog.DescriptionInput?.Trim()
                            });
                            LoadProjectsIntoPanel();
                            UpdateDashboard();
                        }
                    }
                };

                Button btnRemoveProject = new Button
                {
                    Text = "Remove",
                    Location = new Point(460, 35),
                    Size = new Size(80, 25),
                    BackColor = Color.LightCoral
                };

                btnRemoveProject.Click += (s, e) =>
                {
                    var result = MessageBox.Show($"Are you sure you want to delete '{project.Name}'? This will remove all its tasks.", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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

                bool allTasksDone = project.Tasks.Count > 0 && project.Tasks.All(t => t.Status == "Done");

                Button btnMarkAsDone = new Button
                {
                    Text = "Mark as Done",
                    Size = new Size(110, 30),
                    Location = new Point(card.Width - 130, card.Height - 45),
                    BackColor = Color.MediumSeaGreen,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Enabled = allTasksDone
                };
                btnMarkAsDone.FlatAppearance.BorderSize = 0;

                btnMarkAsDone.Click += (s, e) =>
                {
                    var confirm = MessageBox.Show($"Mark '{project.Name}' as completed?", "Confirm Completion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes)
                    {
                        project.IsCompleted = true;
                        SaveProjectsToFile();
                        LoadProjectsIntoPanel();
                        UpdateDashboard();
                        ShowPanel(panelDashboard); // Force refresh dashboard
                        MessageBox.Show("✅ This project has been moved to the History panel.", "Marked as Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };


                Panel taskContainer = new Panel
                {
                    Location = new Point(10, 120),
                    Size = new Size(cardWidth - 20, 140),
                    AutoScroll = true
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
                        bool allDone = project.Tasks.Count > 0 && project.Tasks.All(t => t.Status == "Done");
                        btnMarkAsDone.Enabled = allDone;
                    };

                    Button btnRemoveTask = new Button
                    {
                        Text = "🗑",
                        Location = new Point(300, taskYOffset),
                        Size = new Size(80, 25),
                        BackColor = Color.LightCoral,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnRemoveTask.FlatAppearance.BorderSize = 0;
                    btnRemoveTask.Click += (s, e) =>
                    {
                        var confirm = MessageBox.Show("Delete this task?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (confirm == DialogResult.Yes)
                        {
                            project.Tasks.Remove(task);
                            LoadProjectsIntoPanel();
                            UpdateDashboard();
                        }
                    };

                    taskContainer.Controls.Add(lblTaskTitle);
                    taskContainer.Controls.Add(lblTaskDesc);
                    taskContainer.Controls.Add(cmbStatus);
                    taskContainer.Controls.Add(btnRemoveTask);

                    taskYOffset += 50;
                }

                Color priorityColor;
                if (project.Priority == PriorityLevel.High)
                    priorityColor = Color.Red;
                else if (project.Priority == PriorityLevel.Medium)
                    priorityColor = Color.Orange;
                else if (project.Priority == PriorityLevel.Low)
                    priorityColor = Color.SeaGreen;
                else
                    priorityColor = Color.Gray;

                Label lblPriorityBox = new Label
                {
                    Text = $"Priority: {project.Priority}",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = priorityColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = false,
                    Size = new Size(110, 25),
                    Location = new Point(340, 35),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(2)
                };

                card.Controls.Add(lblTitle);
                card.Controls.Add(lblTimer);
                card.Controls.Add(lblDesc);
                card.Controls.Add(lblDeadline);
                card.Controls.Add(btnStartStop);
                card.Controls.Add(btnAddTask);
                card.Controls.Add(lblPriorityBox);
                card.Controls.Add(btnRemoveProject);
                card.Controls.Add(btnMarkAsDone);
                card.Controls.Add(taskContainer);

                panelProjectsList.Controls.Add(card);
                yOffset += card.Height + 15;
            }
        }

        private void BtnAddProject_Click(object sender, EventArgs e)
        {
            using (var dialog = new ProjectDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string projectName = dialog.ProjectName?.Trim();
                    if (string.IsNullOrWhiteSpace(projectName))
                    {
                        MessageBox.Show("Project name cannot be empty.", "Invalid Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (projects.Any(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A project with this name already exists. Choose another name.", "Duplicate Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (dialog.SelectedDeadline < DateTime.Now.Date)
                    {
                        MessageBox.Show("Deadline cannot be in the past.", "Invalid Deadline", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    projects.Add(new Project
                    {
                        Name = projectName,
                        Description = dialog.ProjectDescription?.Trim(),
                        Deadline = dialog.SelectedDeadline,
                        Priority = dialog.SelectedPriority
                    });
                    SaveProjectsToFile();
                    SetupProjectsPanel();
                    UpdateDashboard();
                    MessageBox.Show("Project created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
