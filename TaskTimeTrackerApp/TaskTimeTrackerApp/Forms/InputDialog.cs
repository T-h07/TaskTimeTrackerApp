using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskTimeTrackerApp
{
    public partial class InputDialog : Form
    {
        public string UserInput { get; private set; }
        public string DescriptionInput { get; private set; }

        private TextBox txtInput;
        private TextBox txtDescription;

        public InputDialog(string title, string prompt)
        {
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(400, 200);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(10, 10),
                Size = new Size(360, 20)
            };

            txtInput = new TextBox
            {
                Location = new Point(10, 35),
                Size = new Size(360, 25)
            };

            Label lblDescription = new Label
            {
                Text = "Description (optional):",
                Location = new Point(10, 65),
                Size = new Size(360, 20)
            };

            txtDescription = new TextBox
            {
                Location = new Point(10, 85),
                Size = new Size(360, 25)
            };

            Button btnOK = new Button
            {
                Text = "OK",
                Location = new Point(200, 120),
                DialogResult = DialogResult.OK
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(290, 120),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(txtInput);
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            btnOK.Click += (s, e) =>
            {
                UserInput = txtInput.Text;
                DescriptionInput = txtDescription.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }
    }
}
