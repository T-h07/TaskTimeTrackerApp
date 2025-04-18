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
        private TextBox txtSearchHistory;

        private void SetupHistoryPanel()
        {
            int leftMargin = 40;
            int topMargin = 70;

            if (cmbSortHistory == null)
            {
                cmbSortHistory = new ComboBox
                {
                    Location = new Point(leftMargin, topMargin),
                    Size = new Size(200, 30),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbSortHistory.Items.AddRange(new[] { "Project Name", "Time Spent", "Number of Tasks" });
                cmbSortHistory.SelectedIndexChanged += (s, e) =>
                    LoadHistoryProjects(txtSearchHistory?.Text?.Trim() ?? "");
                cmbSortHistory.SelectedIndex = 0;
                HistoryPanel.Controls.Add(cmbSortHistory);
            }

            if (txtSearchHistory == null)
            {
                txtSearchHistory = new TextBox
                {
                    Location = new Point(leftMargin + 220, topMargin),
                    Size = new Size(250, 30),
                    Font = new Font("Segoe UI", 10)
                };

                // Emulated placeholder behavior
                txtSearchHistory.Text = "Search by project name...";
                txtSearchHistory.ForeColor = Color.Gray;

                txtSearchHistory.Enter += (s, e) =>
                {
                    if (txtSearchHistory.Text == "Search by project name...")
                    {
                        txtSearchHistory.Text = "";
                        txtSearchHistory.ForeColor = Color.Black;
                    }
                };

                txtSearchHistory.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtSearchHistory.Text))
                    {
                        txtSearchHistory.Text = "Search by project name...";
                        txtSearchHistory.ForeColor = Color.Gray;
                    }
                };

                txtSearchHistory.KeyDown += TxtSearchHistory_KeyDown;
                HistoryPanel.Controls.Add(txtSearchHistory);
            }

            if (panelCompletedProjects == null)
            {
                panelCompletedProjects = new Panel
                {
                    Location = new Point(leftMargin, topMargin + 50),
                    Size = new Size(700, 750),
                    AutoScroll = true,
                    BorderStyle = BorderStyle.FixedSingle
                };
                HistoryPanel.Controls.Add(panelCompletedProjects);
            }

            LoadHistoryProjects(); // Initial load
        }

        private void TxtSearchHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string search = txtSearchHistory.Text.Trim();
                if (search == "Search by project name...") search = "";
                LoadHistoryProjects(search);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LoadHistoryProjects(string searchTerm = "")
        {
            if (panelCompletedProjects == null) return;

            panelCompletedProjects.Controls.Clear();

            var completed = projects
                .Where(p => p.IsCompleted &&
                            (string.IsNullOrWhiteSpace(searchTerm) ||
                             p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            switch (cmbSortHistory?.SelectedItem?.ToString())
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

            if (completed.Count == 0)
            {
                panelCompletedProjects.Controls.Add(new Label
                {
                    Text = "No completed projects found.",
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(10, 10),
                    AutoSize = true
                });
                return;
            }

            int yOffset = 0;

            foreach (var project in completed)
            {
                var card = new Panel
                {
                    Size = new Size(680, 160),
                    Location = new Point(10, yOffset),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                card.Controls.Add(new Label
                {
                    Text = $"Project: {project.Name}",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"Time Tracked: {project.TimeTracked:hh\\:mm\\:ss}",
                    Location = new Point(10, 40),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"Completed Tasks: {project.Tasks.Count(t => t.Status == "Done")}",
                    Location = new Point(10, 70),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = "Tasks: " + string.Join(", ", project.Tasks
                        .Where(t => t.Status == "Done")
                        .Select(t => t.Title)),
                    Location = new Point(10, 100),
                    MaximumSize = new Size(650, 0),
                    AutoSize = true
                });

                panelCompletedProjects.Controls.Add(card);
                yOffset += card.Height + 10;
            }

            // Ensure scroll height is properly applied
            panelCompletedProjects.AutoScrollMinSize = new Size(0, yOffset);
        }
    }
}
