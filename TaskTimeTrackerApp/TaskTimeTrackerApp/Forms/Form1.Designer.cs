namespace TaskTimeTrackerApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.btnHistory = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnProjects = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnTasks = new System.Windows.Forms.Button();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.panelMainContent = new System.Windows.Forms.Panel();
            this.panelDashboard = new System.Windows.Forms.Panel();
            this.panelProjects = new System.Windows.Forms.Panel();
            this.panelTasks = new System.Windows.Forms.Panel();
            this.panelReports = new System.Windows.Forms.Panel();
            this.HistoryPanel = new System.Windows.Forms.Panel();

            this.panelSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelMainContent.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelSidebar
            // 
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.panelSidebar.Controls.Add(this.btnHistory);
            this.panelSidebar.Controls.Add(this.btnReports);
            this.panelSidebar.Controls.Add(this.btnProjects);
            this.panelSidebar.Controls.Add(this.pictureBox1);
            this.panelSidebar.Controls.Add(this.btnTasks);
            this.panelSidebar.Controls.Add(this.btnDashboard);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(192, 961);
            this.panelSidebar.TabIndex = 0;

            // 
            // btnHistory
            // 
            this.btnHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnHistory.Location = new System.Drawing.Point(22, 464);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(145, 35);
            this.btnHistory.TabIndex = 5;
            this.btnHistory.Text = "History";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);

            // 
            // btnReports
            // 
            this.btnReports.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnReports.Location = new System.Drawing.Point(22, 406);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(145, 35);
            this.btnReports.TabIndex = 3;
            this.btnReports.Text = "Reports";
            this.btnReports.UseVisualStyleBackColor = true;
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);

            // 
            // btnProjects
            // 
            this.btnProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnProjects.Location = new System.Drawing.Point(22, 267);
            this.btnProjects.Name = "btnProjects";
            this.btnProjects.Size = new System.Drawing.Size(145, 33);
            this.btnProjects.TabIndex = 1;
            this.btnProjects.Text = "Projects";
            this.btnProjects.UseVisualStyleBackColor = true;
            this.btnProjects.Click += new System.EventHandler(this.btnProjects_Click);

            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(22, 36);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(128, 110);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;

            // 
            // btnTasks
            // 
            this.btnTasks.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnTasks.Location = new System.Drawing.Point(22, 335);
            this.btnTasks.Name = "btnTasks";
            this.btnTasks.Size = new System.Drawing.Size(145, 35);
            this.btnTasks.TabIndex = 2;
            this.btnTasks.Text = "Tasks";
            this.btnTasks.UseVisualStyleBackColor = true;
            this.btnTasks.Click += new System.EventHandler(this.btnTasks_Click);

            // 
            // btnDashboard
            // 
            this.btnDashboard.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnDashboard.Location = new System.Drawing.Point(22, 204);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(145, 33);
            this.btnDashboard.TabIndex = 0;
            this.btnDashboard.Text = "Dashboard";
            this.btnDashboard.UseVisualStyleBackColor = true;
            this.btnDashboard.Click += new System.EventHandler(this.btnDashboard_Click);

            // 
            // panelMainContent
            // 
            this.panelMainContent.Controls.Add(this.panelDashboard);
            this.panelMainContent.Controls.Add(this.panelProjects);
            this.panelMainContent.Controls.Add(this.panelTasks);
            this.panelMainContent.Controls.Add(this.panelReports);
            this.panelMainContent.Controls.Add(this.HistoryPanel); // ✅ FIX: HistoryPanel is now a sibling panel
            this.panelMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainContent.Location = new System.Drawing.Point(192, 0);
            this.panelMainContent.Name = "panelMainContent";
            this.panelMainContent.Size = new System.Drawing.Size(792, 961);
            this.panelMainContent.TabIndex = 1;

            // 
            // panelDashboard
            // 
            this.panelDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDashboard.Location = new System.Drawing.Point(0, 0);
            this.panelDashboard.Name = "panelDashboard";
            this.panelDashboard.Size = new System.Drawing.Size(792, 961);
            this.panelDashboard.TabIndex = 0;

            // 
            // panelProjects
            // 
            this.panelProjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProjects.Location = new System.Drawing.Point(0, 0);
            this.panelProjects.Name = "panelProjects";
            this.panelProjects.Size = new System.Drawing.Size(792, 961);
            this.panelProjects.TabIndex = 1;

            // 
            // panelTasks
            // 
            this.panelTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTasks.Location = new System.Drawing.Point(0, 0);
            this.panelTasks.Name = "panelTasks";
            this.panelTasks.Size = new System.Drawing.Size(792, 961);
            this.panelTasks.TabIndex = 2;

            // 
            // panelReports
            // 
            this.panelReports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelReports.Location = new System.Drawing.Point(0, 0);
            this.panelReports.Name = "panelReports";
            this.panelReports.Size = new System.Drawing.Size(792, 961);
            this.panelReports.TabIndex = 3;

            // 
            // HistoryPanel
            // 
            this.HistoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HistoryPanel.Location = new System.Drawing.Point(0, 0);
            this.HistoryPanel.Name = "HistoryPanel";
            this.HistoryPanel.Size = new System.Drawing.Size(792, 961);
            this.HistoryPanel.TabIndex = 4;

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 961);
            this.Controls.Add(this.panelMainContent);
            this.Controls.Add(this.panelSidebar);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Task & Time Tracker";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelSidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelMainContent.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnTasks;
        private System.Windows.Forms.Button btnProjects;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Panel panelMainContent;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panelDashboard;
        private System.Windows.Forms.Panel panelProjects;
        private System.Windows.Forms.Panel panelTasks;
        private System.Windows.Forms.Panel panelReports;
        private System.Windows.Forms.Panel HistoryPanel;
        private System.Windows.Forms.Button btnHistory;
    }
}
