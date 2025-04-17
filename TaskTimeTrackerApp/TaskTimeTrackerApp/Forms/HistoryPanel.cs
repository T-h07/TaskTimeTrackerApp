using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaskTimeTrackerApp.Models;

namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private ComboBox cmbSortHistory;
        private Panel panelCompletedProjects;

        private void SetupHistoryPanel()
        {
            int leftMargin = 40;
            int topMargin = 70;

            // Initialize ComboBox for sorting
            if (cmbSortHistory == null)
            {
                cmbSortHistory = new ComboBox
                {
                    Location = new Point(leftMargin, topMargin),
                    Size = new Size(200, 30),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbSortHistory.Items.AddRange(new string[] { "Project Name", "Time Spent", "Number of Tasks" });
                cmbSortHistory.SelectedIndexChanged += (s, e) => LoadHistoryProjects();
                cmbSortHistory.SelectedIndex = 0; // default selection
                HistoryPanel.Controls.Add(cmbSortHistory);
            }

            // Initialize the container for completed projects
            if (panelCompletedProjects == null)
            {
                panelCompletedProjects = new Panel
                {
                    Location = new Point(leftMargin, topMargin + 50),
                    Size = new Size(1100, 600),
                    AutoScroll = true
                };
                HistoryPanel.Controls.Add(panelCompletedProjects);
            }

            LoadHistoryProjects();
        }

        private void LoadHistoryProjects()
        {
            if (panelCompletedProjects == null) return;

            panelCompletedProjects.Controls.Clear();

            // If you don't have a Status field in Project, use this:
            var completed = projects
                .Where(p => p.Tasks.Count > 0 && p.Tasks.All(t => t.Status == "Done"))
                .ToList();

            switch (cmbSortHistory.SelectedItem.ToString())
            {
                case "Project Name":
                    completed = completed.OrderBy(p => p.Name).ToList();
                    break;
                case "Time Spent":
                    completed = completed.OrderByDescending(p => p.TimeTracked).ToList();
                    break;
                case "Number of Tasks":
                    completed = completed.OrderByDescending(p => p.Tasks.Count).ToList();
                    break;
            }

            int yOffset = 0;

            foreach (var project in completed)
            {
                Panel card = new Panel
                {
                    Size = new Size(1050, 160),
                    Location = new Point(0, yOffset),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblTitle = new Label
                {
                    Text = $"Project: {project.Name}",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                Label lblTime = new Label
                {
                    Text = $"Time Tracked: {project.TimeTracked:hh\\:mm\\:ss}",
                    Location = new Point(10, 40),
                    AutoSize = true
                };

                Label lblTasks = new Label
                {
                    Text = $"Completed Tasks: {project.Tasks.Count(t => t.Status == "Done")}",
                    Location = new Point(10, 70),
                    AutoSize = true
                };

                Label lblTaskList = new Label
                {
                    Text = "Tasks: " + string.Join(", ", project.Tasks.Where(t => t.Status == "Done").Select(t => t.Title)),
                    Location = new Point(10, 100),
                    AutoSize = true,
                    MaximumSize = new Size(1000, 0)
                };

                card.Controls.Add(lblTitle);
                card.Controls.Add(lblTime);
                card.Controls.Add(lblTasks);
                card.Controls.Add(lblTaskList);

                panelCompletedProjects.Controls.Add(card);

                yOffset += card.Height + 15;
            }
        }
    }
}
