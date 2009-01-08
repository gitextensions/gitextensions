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
            this.Output = new System.Windows.Forms.RichTextBox();
            this.BrowseSource = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Push = new System.Windows.Forms.Button();
            this.PushDestination = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Branch = new System.Windows.Forms.ComboBox();
            this.PushAllBranches = new System.Windows.Forms.CheckBox();
            this.Pull = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Output
            // 
            this.Output.Location = new System.Drawing.Point(22, 97);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(452, 171);
            this.Output.TabIndex = 14;
            this.Output.Text = "";
            // 
            // BrowseSource
            // 
            this.BrowseSource.Location = new System.Drawing.Point(480, 17);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(104, 23);
            this.BrowseSource.TabIndex = 13;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSource_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Destination";
            // 
            // Push
            // 
            this.Push.Location = new System.Drawing.Point(480, 42);
            this.Push.Name = "Push";
            this.Push.Size = new System.Drawing.Size(104, 23);
            this.Push.TabIndex = 15;
            this.Push.Text = "Push";
            this.Push.UseVisualStyleBackColor = true;
            this.Push.Click += new System.EventHandler(this.Push_Click);
            // 
            // PushDestination
            // 
            this.PushDestination.FormattingEnabled = true;
            this.PushDestination.Location = new System.Drawing.Point(109, 19);
            this.PushDestination.Name = "PushDestination";
            this.PushDestination.Size = new System.Drawing.Size(365, 21);
            this.PushDestination.TabIndex = 16;
            this.PushDestination.DropDown += new System.EventHandler(this.PushDestination_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Branch to push";
            // 
            // Branch
            // 
            this.Branch.FormattingEnabled = true;
            this.Branch.Location = new System.Drawing.Point(109, 43);
            this.Branch.Name = "Branch";
            this.Branch.Size = new System.Drawing.Size(365, 21);
            this.Branch.TabIndex = 18;
            this.Branch.DropDown += new System.EventHandler(this.Branch_DropDown);
            // 
            // PushAllBranches
            // 
            this.PushAllBranches.AutoSize = true;
            this.PushAllBranches.Location = new System.Drawing.Point(109, 72);
            this.PushAllBranches.Name = "PushAllBranches";
            this.PushAllBranches.Size = new System.Drawing.Size(110, 17);
            this.PushAllBranches.TabIndex = 19;
            this.PushAllBranches.Text = "Push all branches";
            this.PushAllBranches.UseVisualStyleBackColor = true;
            // 
            // Pull
            // 
            this.Pull.Location = new System.Drawing.Point(480, 245);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(104, 23);
            this.Pull.TabIndex = 20;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.Pull_Click);
            // 
            // FormPush
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 285);
            this.Controls.Add(this.Pull);
            this.Controls.Add(this.PushAllBranches);
            this.Controls.Add(this.Branch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PushDestination);
            this.Controls.Add(this.Push);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.BrowseSource);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPush";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Push";
            this.Load += new System.EventHandler(this.FormPush_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Push;
        private System.Windows.Forms.ComboBox PushDestination;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Branch;
        private System.Windows.Forms.CheckBox PushAllBranches;
        private System.Windows.Forms.Button Pull;
    }
}