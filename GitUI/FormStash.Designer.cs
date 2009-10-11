namespace GitUI
{
    partial class FormStash
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStash));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.Refresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Changes = new System.Windows.Forms.ListBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.StashMessage = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Stashes = new System.Windows.Forms.ComboBox();
            this.gitStashBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.Stashed = new System.Windows.Forms.ListBox();
            this.Stash = new System.Windows.Forms.Button();
            this.Apply = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.View = new GitUI.FileViewer();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitStashBindingSource)).BeginInit();
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
            this.splitContainer1.Panel2.Controls.Add(this.Stash);
            this.splitContainer1.Panel2.Controls.Add(this.Apply);
            this.splitContainer1.Panel2.Controls.Add(this.Clear);
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(708, 520);
            this.splitContainer1.SplitterDistance = 485;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.View);
            this.splitContainer2.Size = new System.Drawing.Size(708, 485);
            this.splitContainer2.SplitterDistance = 251;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer3.Size = new System.Drawing.Size(251, 485);
            this.splitContainer3.SplitterDistance = 206;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.Refresh);
            this.splitContainer4.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.Changes);
            this.splitContainer4.Size = new System.Drawing.Size(251, 206);
            this.splitContainer4.SplitterDistance = 25;
            this.splitContainer4.TabIndex = 0;
            // 
            // Refresh
            // 
            this.Refresh.Location = new System.Drawing.Point(173, 0);
            this.Refresh.Name = "Refresh";
            this.Refresh.Size = new System.Drawing.Size(75, 23);
            this.Refresh.TabIndex = 1;
            this.Refresh.Text = "Refresh";
            this.Refresh.UseVisualStyleBackColor = true;
            this.Refresh.Click += new System.EventHandler(this.Refresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tracked changes in working dir";
            // 
            // Changes
            // 
            this.Changes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Changes.FormattingEnabled = true;
            this.Changes.Location = new System.Drawing.Point(0, 0);
            this.Changes.Name = "Changes";
            this.Changes.Size = new System.Drawing.Size(251, 173);
            this.Changes.TabIndex = 0;
            this.Changes.SelectedIndexChanged += new System.EventHandler(this.Changes_SelectedIndexChanged);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.StashMessage);
            this.splitContainer5.Panel1.Controls.Add(this.label3);
            this.splitContainer5.Panel1.Controls.Add(this.Stashes);
            this.splitContainer5.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.Stashed);
            this.splitContainer5.Size = new System.Drawing.Size(251, 275);
            this.splitContainer5.SplitterDistance = 81;
            this.splitContainer5.TabIndex = 0;
            // 
            // StashMessage
            // 
            this.StashMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StashMessage.Location = new System.Drawing.Point(52, 29);
            this.StashMessage.Name = "StashMessage";
            this.StashMessage.ReadOnly = true;
            this.StashMessage.Size = new System.Drawing.Size(196, 49);
            this.StashMessage.TabIndex = 3;
            this.StashMessage.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Message:";
            // 
            // Stashes
            // 
            this.Stashes.DataSource = this.gitStashBindingSource;
            this.Stashes.DisplayMember = "Name";
            this.Stashes.FormattingEnabled = true;
            this.Stashes.Location = new System.Drawing.Point(52, 3);
            this.Stashes.Name = "Stashes";
            this.Stashes.Size = new System.Drawing.Size(196, 21);
            this.Stashes.TabIndex = 1;
            this.Stashes.SelectedIndexChanged += new System.EventHandler(this.Stashes_SelectedIndexChanged);
            // 
            // gitStashBindingSource
            // 
            this.gitStashBindingSource.DataSource = typeof(GitCommands.GitStash);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Stash:";
            // 
            // Stashed
            // 
            this.Stashed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Stashed.FormattingEnabled = true;
            this.Stashed.Location = new System.Drawing.Point(0, 0);
            this.Stashed.Name = "Stashed";
            this.Stashed.Size = new System.Drawing.Size(251, 186);
            this.Stashed.TabIndex = 0;
            this.Stashed.SelectedIndexChanged += new System.EventHandler(this.Stashed_SelectedIndexChanged);
            // 
            // Stash
            // 
            this.Stash.Location = new System.Drawing.Point(3, 3);
            this.Stash.Name = "Stash";
            this.Stash.Size = new System.Drawing.Size(139, 23);
            this.Stash.TabIndex = 2;
            this.Stash.Text = "Stash all changes";
            this.Stash.UseVisualStyleBackColor = true;
            this.Stash.Click += new System.EventHandler(this.Stash_Click);
            // 
            // Apply
            // 
            this.Apply.Location = new System.Drawing.Point(293, 3);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(185, 23);
            this.Apply.TabIndex = 1;
            this.Apply.Text = "Apply selected stash to working dir";
            this.Apply.UseVisualStyleBackColor = true;
            this.Apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(148, 3);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(139, 23);
            this.Clear.TabIndex = 0;
            this.Clear.Text = "Drop selected stash";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Location = new System.Drawing.Point(0, 0);
            this.View.Name = "View";
            this.View.ScrollPos = 0;
            this.View.Size = new System.Drawing.Size(453, 485);
            this.View.TabIndex = 0;
            // 
            // FormStash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 520);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormStash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Stash";
            this.Load += new System.EventHandler(this.FormStash_Load);
            this.Shown += new System.EventHandler(this.FormStash_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitStashBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.Button Stash;
        private System.Windows.Forms.Button Apply;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox Changes;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox Stashed;
        private System.Windows.Forms.ComboBox Stashes;
        private System.Windows.Forms.BindingSource gitStashBindingSource;
        private System.Windows.Forms.RichTextBox StashMessage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Refresh;
        private FileViewer View;
    }
}