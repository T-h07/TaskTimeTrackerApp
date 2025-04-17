using System;
using System.Drawing;
using System.Windows.Forms;
using TaskTimeTrackerApp.Models;

namespace TaskTimeTrackerApp
{
    public partial class ProjectDialog : Form
    {
        public string ProjectName => txtName.Text;
        public string ProjectDescription => txtDescription.Text;
        public DateTime SelectedDeadline => dtDeadline.Value;
        public PriorityLevel SelectedPriority =>
            Enum.TryParse<PriorityLevel>(cmbPriority.SelectedItem?.ToString(), out var result)
                ? result
                : PriorityLevel.Medium;

        private TextBox txtName;
        private TextBox txtDescription;
        private DateTimePicker dtDeadline;
        private ComboBox cmbPriority;

        public ProjectDialog()
        {
            this.Text = "New Project";
            this.Size = new Size(400, 330);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblName = new Label { Text = "Project Name:", Location = new Point(10, 20), AutoSize = true };
            txtName = new TextBox { Location = new Point(120, 20), Width = 240 };

            Label lblDesc = new Label { Text = "Description:", Location = new Point(10, 60), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(120, 60), Width = 240, Height = 60, Multiline = true };

            Label lblDeadline = new Label { Text = "Deadline:", Location = new Point(10, 135), AutoSize = true };
            dtDeadline = new DateTimePicker
            {
                Location = new Point(120, 130),
                Width = 240,
                Format = DateTimePickerFormat.Short
            };

            Label lblPriority = new Label { Text = "Priority:", Location = new Point(10, 170), AutoSize = true };
            cmbPriority = new ComboBox
            {
                Location = new Point(120, 165),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPriority.Items.AddRange(Enum.GetNames(typeof(PriorityLevel)));
            cmbPriority.SelectedItem = PriorityLevel.Medium.ToString(); // Default

            Button btnOK = new Button { Text = "OK", Location = new Point(200, 220), DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Cancel", Location = new Point(290, 220), DialogResult = DialogResult.Cancel };

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblDeadline);
            this.Controls.Add(dtDeadline);
            this.Controls.Add(lblPriority);
            this.Controls.Add(cmbPriority);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private void ProjectDialog_Load(object sender, EventArgs e)
        {
        }
    }
}
