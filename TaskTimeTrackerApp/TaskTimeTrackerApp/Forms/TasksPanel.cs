using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaskTimeTrackerApp.Models;

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
                            Title = dialog.UserInput,
                            Description = dialog.DescriptionInput
                        });

                        SetupProjectsPanel();
                        SetupTasksPanel();
                        UpdateDashboard();
                    }
                }
            }
        }
    }
}

