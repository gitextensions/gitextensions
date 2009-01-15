namespace GitUI
{
    partial class FormCherryPick
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCherryPick));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.CherryPick = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.AutoCommit);
            this.splitContainer1.Panel2.Controls.Add(this.CherryPick);
            this.splitContainer1.Size = new System.Drawing.Size(638, 387);
            this.splitContainer1.SplitterDistance = 352;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.RevisionGrid);
            this.splitContainer2.Size = new System.Drawing.Size(638, 352);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(477, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select a commit you want to cherry pick. The commit will be recommitted on top of" +
                " the current head.";
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.currentCheckout = null;
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RevisionGrid.Size = new System.Drawing.Size(638, 323);
            this.RevisionGrid.TabIndex = 0;
            // 
            // AutoCommit
            // 
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Checked = true;
            this.AutoCommit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoCommit.Location = new System.Drawing.Point(12, 7);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(332, 17);
            this.AutoCommit.TabIndex = 1;
            this.AutoCommit.Text = "Automatically create a commit  when there are no merge conflicts";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // CherryPick
            // 
            this.CherryPick.Location = new System.Drawing.Point(525, 3);
            this.CherryPick.Name = "CherryPick";
            this.CherryPick.Size = new System.Drawing.Size(110, 23);
            this.CherryPick.TabIndex = 0;
            this.CherryPick.Text = "Cherry pick";
            this.CherryPick.UseVisualStyleBackColor = true;
            this.CherryPick.Click += new System.EventHandler(this.CherryPick_Click);
            // 
            // FormCherryPick
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 387);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCherryPick";
            this.Text = "Cherry pick";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.Button CherryPick;
        private System.Windows.Forms.CheckBox AutoCommit;
    }
}