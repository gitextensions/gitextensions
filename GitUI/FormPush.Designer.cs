namespace GitUI
{
    partial class FormPush
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPush));
            this.BrowseSource = new System.Windows.Forms.Button();
            this.Push = new System.Windows.Forms.Button();
            this.PushDestination = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Branch = new System.Windows.Forms.ComboBox();
            this.PushAllBranches = new System.Windows.Forms.CheckBox();
            this.Pull = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.AddRemote = new System.Windows.Forms.Button();
            this.Remotes = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseSource
            // 
            this.BrowseSource.Enabled = false;
            this.BrowseSource.Location = new System.Drawing.Point(431, 45);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(101, 23);
            this.BrowseSource.TabIndex = 13;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSource_Click);
            // 
            // Push
            // 
            this.Push.Location = new System.Drawing.Point(448, 179);
            this.Push.Name = "Push";
            this.Push.Size = new System.Drawing.Size(101, 23);
            this.Push.TabIndex = 15;
            this.Push.Text = "Push";
            this.Push.UseVisualStyleBackColor = true;
            this.Push.Click += new System.EventHandler(this.Push_Click);
            // 
            // PushDestination
            // 
            this.PushDestination.Enabled = false;
            this.PushDestination.FormattingEnabled = true;
            this.PushDestination.Location = new System.Drawing.Point(128, 46);
            this.PushDestination.Name = "PushDestination";
            this.PushDestination.Size = new System.Drawing.Size(297, 21);
            this.PushDestination.TabIndex = 16;
            this.PushDestination.DropDown += new System.EventHandler(this.PushDestination_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Branch to push";
            // 
            // Branch
            // 
            this.Branch.FormattingEnabled = true;
            this.Branch.Location = new System.Drawing.Point(127, 19);
            this.Branch.Name = "Branch";
            this.Branch.Size = new System.Drawing.Size(297, 21);
            this.Branch.TabIndex = 18;
            this.Branch.DropDown += new System.EventHandler(this.Branch_DropDown);
            // 
            // PushAllBranches
            // 
            this.PushAllBranches.AutoSize = true;
            this.PushAllBranches.Location = new System.Drawing.Point(127, 51);
            this.PushAllBranches.Name = "PushAllBranches";
            this.PushAllBranches.Size = new System.Drawing.Size(110, 17);
            this.PushAllBranches.TabIndex = 19;
            this.PushAllBranches.Text = "Push all branches";
            this.PushAllBranches.UseVisualStyleBackColor = true;
            // 
            // Pull
            // 
            this.Pull.Location = new System.Drawing.Point(13, 179);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(101, 23);
            this.Pull.TabIndex = 20;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.Pull_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.PullFromUrl);
            this.groupBox2.Controls.Add(this.PullFromRemote);
            this.groupBox2.Controls.Add(this.AddRemote);
            this.groupBox2.Controls.Add(this.Remotes);
            this.groupBox2.Controls.Add(this.BrowseSource);
            this.groupBox2.Controls.Add(this.PushDestination);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(541, 80);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Push to";
            // 
            // PullFromUrl
            // 
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 47);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(38, 17);
            this.PullFromUrl.TabIndex = 19;
            this.PullFromUrl.Text = "Url";
            this.PullFromUrl.UseVisualStyleBackColor = true;
            this.PullFromUrl.CheckedChanged += new System.EventHandler(this.PullFromUrl_CheckedChanged);
            // 
            // PullFromRemote
            // 
            this.PullFromRemote.AutoSize = true;
            this.PullFromRemote.Checked = true;
            this.PullFromRemote.Location = new System.Drawing.Point(7, 19);
            this.PullFromRemote.Name = "PullFromRemote";
            this.PullFromRemote.Size = new System.Drawing.Size(110, 17);
            this.PullFromRemote.TabIndex = 18;
            this.PullFromRemote.TabStop = true;
            this.PullFromRemote.Text = "Remote repository";
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemote_CheckedChanged);
            // 
            // AddRemote
            // 
            this.AddRemote.Location = new System.Drawing.Point(431, 18);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(101, 23);
            this.AddRemote.TabIndex = 17;
            this.AddRemote.Text = "Add remote";
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemote_Click);
            // 
            // Remotes
            // 
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(128, 19);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(297, 21);
            this.Remotes.TabIndex = 16;
            this.Remotes.DropDown += new System.EventHandler(this.Remotes_DropDown);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Branch);
            this.groupBox1.Controls.Add(this.PushAllBranches);
            this.groupBox1.Location = new System.Drawing.Point(13, 99);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(540, 74);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Branch";
            // 
            // FormPush
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 213);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.Push);
            this.Controls.Add(this.Pull);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPush";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Push";
            this.Load += new System.EventHandler(this.FormPush_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.Button Push;
        private System.Windows.Forms.ComboBox PushDestination;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Branch;
        private System.Windows.Forms.CheckBox PushAllBranches;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton PullFromUrl;
        private System.Windows.Forms.RadioButton PullFromRemote;
        private System.Windows.Forms.Button AddRemote;
        private System.Windows.Forms.ComboBox Remotes;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}