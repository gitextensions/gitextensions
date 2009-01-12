namespace GitUI
{
    partial class FormRemotes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRemotes));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Remotes = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.RemoteName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Url = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.New = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Remotes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.Delete);
            this.splitContainer1.Panel2.Controls.Add(this.New);
            this.splitContainer1.Panel2.Controls.Add(this.Save);
            this.splitContainer1.Size = new System.Drawing.Size(606, 212);
            this.splitContainer1.SplitterDistance = 183;
            this.splitContainer1.TabIndex = 0;
            // 
            // Remotes
            // 
            this.Remotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(0, 0);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(183, 212);
            this.Remotes.TabIndex = 0;
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.Remotes_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // RemoteName
            // 
            this.RemoteName.Location = new System.Drawing.Point(101, 19);
            this.RemoteName.Name = "RemoteName";
            this.RemoteName.Size = new System.Drawing.Size(221, 20);
            this.RemoteName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Url";
            // 
            // Url
            // 
            this.Url.FormattingEnabled = true;
            this.Url.Location = new System.Drawing.Point(101, 48);
            this.Url.Name = "Url";
            this.Url.Size = new System.Drawing.Size(221, 21);
            this.Url.TabIndex = 3;
            this.Url.DropDown += new System.EventHandler(this.Url_DropDown);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(328, 46);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 4;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(339, 177);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 5;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // New
            // 
            this.New.Location = new System.Drawing.Point(258, 177);
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(75, 23);
            this.New.TabIndex = 6;
            this.New.Text = "New";
            this.New.UseVisualStyleBackColor = true;
            this.New.Click += new System.EventHandler(this.New_Click);
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(177, 177);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 7;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RemoteName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Url);
            this.groupBox1.Controls.Add(this.Browse);
            this.groupBox1.Location = new System.Drawing.Point(2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(412, 168);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            // 
            // FormRemotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 212);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormRemotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repositories";
            this.Load += new System.EventHandler(this.FormRemotes_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox Remotes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.ComboBox Url;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox RemoteName;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button New;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}