using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaskTimeTrackerApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;


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
                                Description = inputDialog.DescriptionInput
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
                    SetupProjectsPanel();
                    UpdateDashboard();
                }
            }
        }
    }
}
