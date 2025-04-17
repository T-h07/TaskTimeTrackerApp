using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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

                    using (var doc = new PDF.Document())
                    {
                        PDFWriter.PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                        doc.Open();

                        var headingFont = new PDF.Font(PDF.Font.FontFamily.HELVETICA, 14, PDF.Font.BOLD);
                        var normalFont = new PDF.Font(PDF.Font.FontFamily.HELVETICA, 11);

                        doc.Add(new PDF.Paragraph("Project Report", headingFont));
                        doc.Add(new PDF.Paragraph("\n", normalFont));
                        doc.Add(new PDF.Paragraph($"Project Name: {project.Name}", normalFont));
                        doc.Add(new PDF.Paragraph($"Description: {project.Description}", normalFont));
                        doc.Add(new PDF.Paragraph($"Time Tracked: {project.TimeTracked}", normalFont));
                        doc.Add(new PDF.Paragraph($"Estimated Days: {(int)Math.Ceiling(project.TimeTracked.TotalDays)} day(s)", normalFont));

                        doc.Add(new PDF.Paragraph("\nTasks:", headingFont));
                        foreach (var task in project.Tasks)
                        {
                            doc.Add(new PDF.Paragraph($"- {task.Title} [{task.Status}] - {task.Description}", normalFont));
                        }

                        doc.Close();
                    }

                    MessageBox.Show($"Report saved to:\n{filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }




    }
}
