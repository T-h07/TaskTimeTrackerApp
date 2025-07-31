using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TaskTimeTrackerApp.Models;
using PDF = iTextSharp.text;
using PDFWriter = iTextSharp.text.pdf;
using Newtonsoft.Json;

namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        private ComboBox cmbReportProjects;
        private RichTextBox txtProjectReportDesc;
        private Panel panelReportTaskBoxes;
        private Label lblReportTime;
        private Label lblReportDays;
        private ProgressBar reportProgressBar;
        private Button btnDownloadReport;
        private Chart chartReport;

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
            cmbReportProjects.Items.Add("All Projects");
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
                Size = new Size(panelReports.Width - 40, 250),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelReports.Controls.Add(panelReportTaskBoxes);

            chartReport = new Chart
            {
                Location = new Point(20, 540),
                Size = new Size(700, 300),
                BorderlineColor = Color.Gray,
                BorderlineDashStyle = ChartDashStyle.Solid,
                BorderlineWidth = 1
            };
            chartReport.ChartAreas.Add(new ChartArea());
            panelReports.Controls.Add(chartReport);

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
            chartReport.Series.Clear();
            chartReport.Titles.Clear();
            panelReportTaskBoxes.Controls.Clear();

            if (selectedProjectName == "All Projects")
            {
                // Global Report
                var activeProjects = projects.Where(p => !p.IsCompleted).ToList();
                txtProjectReportDesc.Text = "Combined report for all active projects.";
                lblReportTime.Text = $"Total Time: {activeProjects.Sum(p => p.TimeTracked.TotalHours):0.0} hours";
                lblReportDays.Text = $"Projects: {activeProjects.Count}";
                reportProgressBar.Value = 0;

                chartReport.Titles.Add("Time Spent per Project");
                Series series = new Series
                {
                    ChartType = SeriesChartType.Bar,
                    IsValueShownAsLabel = true
                };

                foreach (var proj in activeProjects.OrderByDescending(p => p.TimeTracked.TotalHours))
                {
                    double value = proj.TimeTracked.TotalHours;
                    var point = series.Points.AddXY(proj.Name, value);
                    series.Points.Last().Label = $"{(int)proj.TimeTracked.TotalHours}h {proj.TimeTracked.Minutes}m";
                }

                chartReport.Series.Add(series);
                chartReport.ChartAreas[0].AxisX.Title = "Time (hours)";
                chartReport.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartReport.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            }
            else
            {
                // Single Project Report
                var project = projects.FirstOrDefault(p => p.Name == selectedProjectName);
                if (project == null) return;

                txtProjectReportDesc.Text = project.Description;
                lblReportTime.Text = $"Time Tracked: {project.TimeTracked:hh\\:mm\\:ss}";
                lblReportDays.Text = $"~{(int)Math.Ceiling(project.TimeTracked.TotalDays)} day(s)";

                int total = project.Tasks.Count;
                int done = project.Tasks.Count(t => t.Status == "Done");
                reportProgressBar.Value = total > 0 ? (int)(((double)done / total) * 100) : 0;

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
                        Text = string.IsNullOrWhiteSpace(task.Description) ? "(No Description)" : task.Description,
                        Font = new Font("Segoe UI", 9),
                        Location = new Point(10, 25),
                        AutoSize = true
                    };

                    taskBox.Controls.Add(lblTitle);
                    taskBox.Controls.Add(lblDesc);
                    panelReportTaskBoxes.Controls.Add(taskBox);
                    yOffset += 80;
                }

                chartReport.Titles.Add("Task Status Breakdown");
                Series series = new Series
                {
                    ChartType = SeriesChartType.Pie,
                    IsValueShownAsLabel = true,
                    Label = "#VALX (#VALY)",
                    LegendText = "#VALX"
                };

                int toDo = project.Tasks.Count(t => t.Status == "To Do");
                int inProgress = project.Tasks.Count(t => t.Status == "In Progress");
                int doneCount = project.Tasks.Count(t => t.Status == "Done");

                series.Points.AddXY($"To Do ({toDo})", toDo);
                series.Points.AddXY($"In Progress ({inProgress})", inProgress);
                series.Points.AddXY($"Done ({doneCount})", doneCount);

                chartReport.Series.Add(series);
            }
        }

        // Save chart as image and embed into PDF
        private void AddChartToPdf(PDF.Document doc, Chart chart)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Png);
                iTextSharp.text.Image chartImage = iTextSharp.text.Image.GetInstance(ms.ToArray());
                chartImage.Alignment = PDF.Element.ALIGN_CENTER;
                chartImage.ScaleToFit(500f, 300f);
                doc.Add(chartImage);
            }
        }

        // Create a time chart for PDF



        // Helper method for clean time display
        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
                return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }

        private Chart CreateTimeChart(Project project)
        {
            Chart chart = new Chart { Width = 600, Height = 300, BackColor = Color.White };
            ChartArea area = new ChartArea();
            chart.ChartAreas.Add(area);

            Series series = new Series
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true
            };

            double value = project.TimeTracked.TotalHours; // Default in hours
            string axisTitle = "Hours (decimal)";

            // Adjust unit dynamically
            if (project.TimeTracked.TotalHours < 1)
            {
                value = project.TimeTracked.TotalMinutes;
                axisTitle = "Minutes";
            }
            else if (project.TimeTracked.TotalDays >= 1)
            {
                value = project.TimeTracked.TotalDays;
                axisTitle = "Days";
            }

            int index = series.Points.AddXY("Time Spent", value);
            series.Points[index].Label = FormatTimeSpan(project.TimeTracked);

            chart.Series.Add(series);
            chart.Titles.Add("Project Time Spent");

            // Axis formatting
            area.AxisY.Title = axisTitle;
            area.AxisY.LabelStyle.Format = "0";
            area.AxisX.LabelStyle.Angle = -15;

            return chart;
        }



        private void BtnDownloadReport_Click(object sender, EventArgs e)
        {
            if (cmbReportProjects.SelectedItem == null) return;

            string selectedProjectName = cmbReportProjects.SelectedItem.ToString();
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save PDF Report";
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.FileName = selectedProjectName == "All Projects" ? "All_Projects_Report.pdf" : $"{selectedProjectName}_Report.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var doc = new PDF.Document())
                    {
                        PDFWriter.PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                        doc.Open();

                        var headingFont = new PDF.Font(PDF.Font.FontFamily.HELVETICA, 14, PDF.Font.BOLD);
                        var normalFont = new PDF.Font(PDF.Font.FontFamily.HELVETICA, 11);

                        doc.Add(new PDF.Paragraph("Project Report", headingFont));
                        doc.Add(new PDF.Paragraph("\n", normalFont));

                        if (selectedProjectName == "All Projects")
                        {
                            doc.Add(new PDF.Paragraph("Combined Report for All Projects", headingFont));
                            foreach (var proj in projects)
                            {
                                doc.Add(new PDF.Paragraph($"\nProject Name: {proj.Name}", normalFont));
                                doc.Add(new PDF.Paragraph($"Description: {proj.Description}", normalFont));
                                doc.Add(new PDF.Paragraph($"Time Tracked: {FormatTimeSpan(proj.TimeTracked)}", normalFont));
                                doc.Add(new PDF.Paragraph($"Tasks: {proj.Tasks.Count} (Done: {proj.Tasks.Count(t => t.Status == "Done")})", normalFont));
                            }
                            AddChartToPdf(doc, chartReport);
                        }
                        else
                        {
                            var project = projects.FirstOrDefault(p => p.Name == selectedProjectName);
                            if (project == null) return;

                            doc.Add(new PDF.Paragraph($"Project Name: {project.Name}", normalFont));
                            doc.Add(new PDF.Paragraph($"Description: {project.Description}", normalFont));
                            doc.Add(new PDF.Paragraph($"Time Tracked: {FormatTimeSpan(project.TimeTracked)}", normalFont));
                            doc.Add(new PDF.Paragraph($"Estimated Days: {(int)Math.Ceiling(project.TimeTracked.TotalDays)} day(s)", normalFont));

                            doc.Add(new PDF.Paragraph("\nTasks:", headingFont));
                            foreach (var task in project.Tasks)
                            {
                                doc.Add(new PDF.Paragraph($"- {task.Title} [{task.Status}] - {task.Description}", normalFont));
                            }

                            // Add charts to PDF
                            AddChartToPdf(doc, chartReport); // Task breakdown pie chart
                            AddChartToPdf(doc, CreateTimeChart(project)); // Time spent chart
                        }

                        doc.Close();
                    }

                    MessageBox.Show($"Report saved to:\n{filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

    }
}
