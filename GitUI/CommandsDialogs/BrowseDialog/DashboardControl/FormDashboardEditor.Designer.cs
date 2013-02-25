namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class FormDashboardEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dashboardEditor1 = new DashboardEditor();
            this.SuspendLayout();
            // 
            // dashboardEditor1
            // 
            this.dashboardEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardEditor1.Location = new System.Drawing.Point(0, 0);
            this.dashboardEditor1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dashboardEditor1.Name = "dashboardEditor1";
            this.dashboardEditor1.Size = new System.Drawing.Size(698, 447);
            this.dashboardEditor1.TabIndex = 0;
            // 
            // FormDashboardEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(698, 447);
            this.Controls.Add(this.dashboardEditor1);
            this.Name = "FormDashboardEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Start Page";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDashboardEditor_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private DashboardEditor dashboardEditor1;
    }
}