using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskTimeTrackerApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ShowPanel(panelDashboard); // Default panel on load
            AddLabelsToPanels();       // Add header labels to each panel
        }

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
        }

        private void btnProjects_Click(object sender, EventArgs e)
        {
            ShowPanel(panelProjects);
        }

        private void btnTasks_Click(object sender, EventArgs e)
        {
            ShowPanel(panelTasks);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            ShowPanel(panelReports);
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
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lbl.ForeColor = Color.DarkSlateGray;
            lbl.Location = new Point(20, 20);
            lbl.AutoSize = true;
            panel.Controls.Add(lbl);
        }
    }
}