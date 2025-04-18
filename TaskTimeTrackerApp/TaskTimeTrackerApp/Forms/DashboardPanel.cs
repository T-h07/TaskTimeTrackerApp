using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TaskTimeTrackerApp.Models;
using Newtonsoft.Json;



namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private Label lblTotalProjects;
        private Label lblTotalTasks;
        private Label lblTasksInProgress;
        private Label lblTimeToday;

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

        private void UpdateDashboard()
        {
            lblTotalProjects.Text = projects.Count.ToString();
            lblTotalTasks.Text = projects.Sum(p => p.Tasks.Count).ToString();
            lblTasksInProgress.Text = projects.Sum(p => p.Tasks.Count(t => t.Status == "In Progress")).ToString();
            lblTimeToday.Text = $"{projects.Sum(p => p.TimeTracked.TotalHours):0}h {projects.Sum(p => p.TimeTracked.Minutes):00}m";

            // Clear and re-add Motivation
            panelDashboard.Controls.OfType<Panel>()
                .Where(p => p.Controls.OfType<Label>().Any(l => l.Text.StartsWith("Motivational Quote")))
                .ToList()
                .ForEach(p => panelDashboard.Controls.Remove(p));
            CreateMotivationAndDeadlines(panelDashboard, new Point(650, 460));

            // Clear and re-add Recent Tasks
            panelDashboard.Controls.OfType<Panel>()
                .Where(p => p.Controls.OfType<Label>().Any(l => l.Text == "Recent Tasks"))
                .ToList()
                .ForEach(p => panelDashboard.Controls.Remove(p));
            CreateRecentTasksBox(panelDashboard, new Point(30, 240));

            // Clear and re-add Pie Chart
            panelDashboard.Controls.OfType<Chart>().ToList()
                .ForEach(c => panelDashboard.Controls.Remove(c));
            CreatePieChartBox(panelDashboard, new Point(650, 240));

            // Clear and re-add Progress Tracker
            panelDashboard.Controls.OfType<Panel>()
                .Where(p => p.Controls.OfType<Label>().Any(l => l.Text == "Progress Tracker"))
                .ToList()
                .ForEach(p => panelDashboard.Controls.Remove(p));
            CreateProgressTracker(panelDashboard, new Point(30, 420));
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

            var recent = projects
                .SelectMany(p => p.Tasks.Select(t => new { Project = p.Name, Task = t }))
                .Where(x => x.Task.Status != "Done")
                .OrderByDescending(x => x.Task.Title)
                .Take(5)
                .Select(x => $"{x.Task.Title} [{x.Task.Status}] - {x.Project}")
                .ToArray();

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
                Font = new Font("Segoe UI", 9),
                IsValueShownAsLabel = true,
                Label = "#PERCENT{P0}",
                LegendText = "#VALX"
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

            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Font = new Font("Segoe UI", 9),
                Alignment = StringAlignment.Center
            };
            chart.Legends.Add(legend);

            parent.Controls.Add(chart);
        }

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

            deadlinesLabel = new Label
            {
                Font = new Font("Segoe UI", 10),
                MaximumSize = new Size(280, 0),
                AutoSize = true
            };

            box.Controls.Add(deadlinesLabel);
            parent.Controls.Add(box);

            UpdateDeadlinesLabel();
            StartMotivationalQuoteTimer();
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
            quoteTimer.Interval = 10000;
            quoteTimer.Tick += (s, e) =>
            {
                currentQuoteIndex = (currentQuoteIndex + 1) % motivationalQuotes.Count;
                if (lblMotivationalQuote != null)
                {
                    lblMotivationalQuote.Text = "Motivational Quote:\n" + motivationalQuotes[currentQuoteIndex];
                    UpdateDeadlinesLabel();
                }
            };
            quoteTimer.Start();
        }

        private void CreateProgressTracker(Control parent, Point location)
        {
            Panel container = new Panel
            {
                BackColor = Color.White,
                Size = new Size(600, 250),
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

            // Legend
            int legendTop = titleLabel.Bottom + 5;

            void AddLegend(Color color, string labelText, int offsetX)
            {
                Panel colorBox = new Panel
                {
                    BackColor = color,
                    Size = new Size(15, 15),
                    Location = new Point(offsetX, legendTop)
                };

                Label label = new Label
                {
                    Text = labelText,
                    Location = new Point(offsetX + 20, legendTop - 2),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9)
                };

                container.Controls.Add(colorBox);
                container.Controls.Add(label);
            }

            AddLegend(Color.Green, "Completed", 10);
            AddLegend(Color.Orange, "In Progress", 120);
            AddLegend(Color.LightGray, "Remaining", 250);

            int yOffset = legendTop + 25;

            foreach (var project in projects.Where(p => p.Tasks.Any(t => t.Status != "Done")))
            {
                int total = project.Tasks.Count;
                if (total == 0) continue;

                int done = project.Tasks.Count(t => t.Status == "Done");
                int inProgress = project.Tasks.Count(t => t.Status == "In Progress");

                Label taskLabel = new Label
                {
                    Text = $"{project.Name} ({done}/{total} tasks completed)",
                    Location = new Point(10, yOffset),
                    AutoSize = true
                };
                container.Controls.Add(taskLabel);

                Panel progressPanel = new Panel
                {
                    Location = new Point(10, yOffset + 20),
                    Size = new Size(560, 20),
                    BorderStyle = BorderStyle.FixedSingle
                };

                progressPanel.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    Rectangle bounds = progressPanel.ClientRectangle;

                    int greenWidth = (int)(bounds.Width * ((double)done / total));
                    int orangeWidth = (int)(bounds.Width * ((double)inProgress / total));
                    int remainingWidth = bounds.Width - greenWidth - orangeWidth;

                    g.FillRectangle(Brushes.Green, 0, 0, greenWidth, bounds.Height);
                    g.FillRectangle(Brushes.Orange, greenWidth, 0, orangeWidth, bounds.Height);
                    g.FillRectangle(Brushes.LightGray, greenWidth + orangeWidth, 0, remainingWidth, bounds.Height);
                };

                container.Controls.Add(progressPanel);
                yOffset += 50;
            }


            parent.Controls.Add(container);
        }


    }
}

