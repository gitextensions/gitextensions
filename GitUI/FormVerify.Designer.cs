namespace GitUI
{
    partial class FormVerify
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVerify));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.ShowOnlyCommits = new System.Windows.Forms.CheckBox();
            this.NoReflogs = new System.Windows.Forms.CheckBox();
            this.FullCheck = new System.Windows.Forms.CheckBox();
            this.Unreachable = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Warnings = new System.Windows.Forms.ListBox();
            this.TagAllCommits = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.TagAllObjects = new System.Windows.Forms.Button();
            this.ViewObject = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Remove = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
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
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.TagAllCommits);
            this.splitContainer1.Panel2.Controls.Add(this.button3);
            this.splitContainer1.Panel2.Controls.Add(this.TagAllObjects);
            this.splitContainer1.Panel2.Controls.Add(this.ViewObject);
            this.splitContainer1.Panel2.Controls.Add(this.button2);
            this.splitContainer1.Panel2.Controls.Add(this.Remove);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Size = new System.Drawing.Size(820, 524);
            this.splitContainer1.SplitterDistance = 466;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.ShowOnlyCommits);
            this.splitContainer2.Panel1.Controls.Add(this.NoReflogs);
            this.splitContainer2.Panel1.Controls.Add(this.FullCheck);
            this.splitContainer2.Panel1.Controls.Add(this.Unreachable);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
            this.splitContainer2.Panel2.Controls.Add(this.Warnings);
            this.splitContainer2.Size = new System.Drawing.Size(820, 466);
            this.splitContainer2.SplitterDistance = 137;
            this.splitContainer2.TabIndex = 0;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(280, 91);
            this.label2.TabIndex = 5;
            // 
            // ShowOnlyCommits
            // 
            resources.ApplyResources(this.ShowOnlyCommits, "ShowOnlyCommits");
            this.ShowOnlyCommits.AutoSize = true;
            this.ShowOnlyCommits.Checked = true;
            this.ShowOnlyCommits.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowOnlyCommits.Location = new System.Drawing.Point(430, 3);
            this.ShowOnlyCommits.Name = "ShowOnlyCommits";
            this.ShowOnlyCommits.Size = new System.Drawing.Size(116, 17);
            this.ShowOnlyCommits.TabIndex = 4;
            this.ShowOnlyCommits.UseVisualStyleBackColor = true;
            this.ShowOnlyCommits.CheckedChanged += new System.EventHandler(this.ShowOnlyCommits_CheckedChanged);
            // 
            // NoReflogs
            // 
            resources.ApplyResources(this.NoReflogs, "NoReflogs");
            this.NoReflogs.AutoSize = true;
            this.NoReflogs.Checked = true;
            this.NoReflogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NoReflogs.Location = new System.Drawing.Point(430, 29);
            this.NoReflogs.Name = "NoReflogs";
            this.NoReflogs.Size = new System.Drawing.Size(335, 30);
            this.NoReflogs.TabIndex = 3;
            this.NoReflogs.UseVisualStyleBackColor = true;
            this.NoReflogs.CheckedChanged += new System.EventHandler(this.NoReflogs_CheckedChanged);
            // 
            // FullCheck
            // 
            resources.ApplyResources(this.FullCheck, "FullCheck");
            this.FullCheck.AutoSize = true;
            this.FullCheck.Location = new System.Drawing.Point(430, 95);
            this.FullCheck.Name = "FullCheck";
            this.FullCheck.Size = new System.Drawing.Size(382, 30);
            this.FullCheck.TabIndex = 2;
            this.FullCheck.UseVisualStyleBackColor = true;
            this.FullCheck.CheckedChanged += new System.EventHandler(this.FullCheck_CheckedChanged);
            // 
            // Unreachable
            // 
            resources.ApplyResources(this.Unreachable, "Unreachable");
            this.Unreachable.AutoSize = true;
            this.Unreachable.Location = new System.Drawing.Point(430, 62);
            this.Unreachable.Name = "Unreachable";
            this.Unreachable.Size = new System.Drawing.Size(383, 30);
            this.Unreachable.TabIndex = 1;
            this.Unreachable.UseVisualStyleBackColor = true;
            this.Unreachable.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(269, 13);
            this.label1.TabIndex = 0;
            // 
            // Warnings
            // 
            resources.ApplyResources(this.Warnings, "Warnings");
            this.Warnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Warnings.FormattingEnabled = true;
            this.Warnings.ItemHeight = 12;
            this.Warnings.Location = new System.Drawing.Point(0, 0);
            this.Warnings.Name = "Warnings";
            this.Warnings.Size = new System.Drawing.Size(820, 316);
            this.Warnings.TabIndex = 0;
            this.Warnings.DoubleClick += new System.EventHandler(this.Warnings_DoubleClick);
            // 
            // TagAllCommits
            // 
            resources.ApplyResources(this.TagAllCommits, "TagAllCommits");
            this.TagAllCommits.Location = new System.Drawing.Point(300, 3);
            this.TagAllCommits.Name = "TagAllCommits";
            this.TagAllCommits.Size = new System.Drawing.Size(173, 23);
            this.TagAllCommits.TabIndex = 6;
            this.TagAllCommits.UseVisualStyleBackColor = true;
            this.TagAllCommits.Click += new System.EventHandler(this.TagAllCommits_Click);
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Location = new System.Drawing.Point(4, 29);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(290, 23);
            this.button3.TabIndex = 5;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // TagAllObjects
            // 
            resources.ApplyResources(this.TagAllObjects, "TagAllObjects");
            this.TagAllObjects.Location = new System.Drawing.Point(150, 3);
            this.TagAllObjects.Name = "TagAllObjects";
            this.TagAllObjects.Size = new System.Drawing.Size(144, 23);
            this.TagAllObjects.TabIndex = 4;
            this.TagAllObjects.UseVisualStyleBackColor = true;
            this.TagAllObjects.Click += new System.EventHandler(this.TagAllObjects_Click);
            // 
            // ViewObject
            // 
            resources.ApplyResources(this.ViewObject, "ViewObject");
            this.ViewObject.Location = new System.Drawing.Point(300, 29);
            this.ViewObject.Name = "ViewObject";
            this.ViewObject.Size = new System.Drawing.Size(173, 23);
            this.ViewObject.TabIndex = 3;
            this.ViewObject.UseVisualStyleBackColor = true;
            this.ViewObject.Click += new System.EventHandler(this.ViewObject_Click);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Location = new System.Drawing.Point(3, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(141, 23);
            this.button2.TabIndex = 2;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Remove
            // 
            resources.ApplyResources(this.Remove, "Remove");
            this.Remove.Location = new System.Drawing.Point(610, 3);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(207, 23);
            this.Remove.TabIndex = 1;
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Location = new System.Drawing.Point(610, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(208, 23);
            this.button1.TabIndex = 0;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormVerify
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 524);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            //this.Icon = global::GitUI.Properties.Resources.cow_head;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormVerify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.FormVerify_Load);
            this.Shown += new System.EventHandler(this.FormVerify_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox Warnings;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button ViewObject;
        private System.Windows.Forms.CheckBox Unreachable;
        private System.Windows.Forms.Button TagAllObjects;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox FullCheck;
        private System.Windows.Forms.CheckBox NoReflogs;
        private System.Windows.Forms.Button TagAllCommits;
        private System.Windows.Forms.CheckBox ShowOnlyCommits;
        private System.Windows.Forms.Label label2;
    }
}