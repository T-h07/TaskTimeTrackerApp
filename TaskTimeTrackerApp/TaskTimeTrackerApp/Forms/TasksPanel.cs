using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaskTimeTrackerApp.Models;
using Newtonsoft.Json;


namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private ComboBox cmbProjectFilter;
        private ComboBox cmbStatusFilter;
        private TextBox txtSearch = new TextBox();
        private Panel panelTaskList;
        private Button btnAddTask;

        private void SetupTasksPanel()
        {
            panelTasks.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "All Tasks",
                Font = new Font("Segoe UI", 20, FontStyle.Bold), // Bigger title
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelTasks.Controls.Add(lblTitle);

            txtSearch = new TextBox
            {
                Location = new Point(20, 70),
                Size = new Size(220, 32), // Larger height
                Font = new Font("Segoe UI", 11),
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
                Location = new Point(260, 70),
                Size = new Size(200, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProjectFilter.Items.Add("All Projects");
            cmbProjectFilter.Items.AddRange(projects.Select(p => p.Name).ToArray());
            cmbProjectFilter.SelectedIndex = 0;
            cmbProjectFilter.SelectedIndexChanged += (s, e) => RefreshTaskList();
            panelTasks.Controls.Add(cmbProjectFilter);

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(470, 70),
                Size = new Size(150, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatusFilter.Items.AddRange(new[] { "All", "To Do", "In Progress", "Done" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => RefreshTaskList();
            panelTasks.Controls.Add(cmbStatusFilter);

            btnAddTask = new Button
            {
                Text = "➕ Add Task",
                Location = new Point(640, 70),
                Size = new Size(140, 32),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.LightGreen
            };
            btnAddTask.Click += BtnAddTask_Click;
            panelTasks.Controls.Add(btnAddTask);

            panelTaskList = new Panel
            {
                Location = new Point(20, 120), // Pushed down to match larger controls
                Size = new Size(panelTasks.Width - 40, panelTasks.Height - 150),
                AutoScroll = true
            };
            panelTasks.Controls.Add(panelTaskList);

            RefreshTaskList();
        }


        private void RefreshTaskList()
        {
            panelTaskList.Controls.Clear();

            string searchText = txtSearch.Text.ToLower();
            if (searchText == "search tasks...") searchText = "";

            string selectedProject = cmbProjectFilter.SelectedItem?.ToString();
            string selectedStatus = cmbStatusFilter.SelectedItem?.ToString();

            var filteredTasks = new System.Collections.Generic.List<(Project Project, TaskItem Task)>();

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
            int rowIndex = 0;
            foreach (var (project, task) in filteredTasks)
            {
                // Create "card-like" panel
                Panel taskCard = new Panel
                {
                    Location = new Point(10, yOffset),
                    Size = new Size(panelTaskList.Width - 40, 70),
                    BackColor = rowIndex % 2 == 0 ? Color.White : Color.FromArgb(245, 245, 245),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Rounded corners
                taskCard.Paint += (s, e) =>
                {
                    using (var brush = new SolidBrush(taskCard.BackColor))
                    using (var pen = new Pen(Color.LightGray))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        var rect = taskCard.ClientRectangle;
                        int radius = 8;
                        var path = new System.Drawing.Drawing2D.GraphicsPath();
                        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                        path.CloseAllFigures();
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                };

                // Task title
                Label lblTitle = new Label
                {
                    Text = $"[{project.Name}] {task.Title}",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(10, 8),
                    MaximumSize = new Size(taskCard.Width - 160, 0),
                    AutoSize = true
                };

                // Status dropdown
                ComboBox cmbStatus = new ComboBox
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(taskCard.Width - 150, 20),
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

                // Delete button
                Button btnRemove = new Button
                {
                    Text = "🗑️",
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(taskCard.Width - 45, 20), // 10px gap from right edge
                    Size = new Size(30, 25),
                    BackColor = Color.LightCoral,
                    FlatStyle = FlatStyle.Flat
                };
                btnRemove.FlatAppearance.BorderSize = 0;
                btnRemove.Click += (s, e) =>
                {
                    project.Tasks.Remove(task);
                    RefreshTaskList();
                    LoadProjectsIntoPanel();
                    UpdateDashboard();
                };

                // Add controls
                taskCard.Controls.Add(lblTitle);
                taskCard.Controls.Add(cmbStatus);
                taskCard.Controls.Add(btnRemove);

                panelTaskList.Controls.Add(taskCard);
                yOffset += 80;
                rowIndex++;
            }
        }



        private void BtnAddTask_Click(object sender, EventArgs e)
        {
            var dialog = new InputDialog("New Task", "Enter task title:");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string taskName = dialog.UserInput?.Trim();
                if (string.IsNullOrWhiteSpace(taskName))
                {
                    MessageBox.Show("Task name cannot be empty.", "Invalid Task", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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

                // Only show active (not completed) projects
                projectBox.Items.AddRange(projects
                    .Where(p => !p.IsCompleted)
                    .Select(p => p.Name)
                    .ToArray());
                if (projectBox.Items.Count > 0)
                    projectBox.SelectedIndex = 0;
                else
                {
                    MessageBox.Show("No active projects available. Create a new project first.", "No Projects", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

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
                            Title = taskName,
                            Description = dialog.DescriptionInput?.Trim()
                        });

                        SaveProjectsToFile();
                        SetupProjectsPanel();
                        SetupTasksPanel();
                        UpdateDashboard();
                    }
                }
            }
        }

    }
}

