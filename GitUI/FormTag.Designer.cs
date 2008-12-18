namespace GitUI
{
    partial class FormTag
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTag));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.GitRevisions = new GitUI.RevisionGrid();
            this.Tagname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CreateTag = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.GitRevisions);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Tagname);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.CreateTag);
            this.splitContainer1.Size = new System.Drawing.Size(734, 523);
            this.splitContainer1.SplitterDistance = 489;
            this.splitContainer1.TabIndex = 0;
            // 
            // GitRevisions
            // 
            this.GitRevisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitRevisions.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.GitRevisions.Location = new System.Drawing.Point(0, 0);
            this.GitRevisions.Name = "GitRevisions";
            this.GitRevisions.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GitRevisions.Size = new System.Drawing.Size(734, 489);
            this.GitRevisions.TabIndex = 0;
            // 
            // Tagname
            // 
            this.Tagname.Location = new System.Drawing.Point(71, 5);
            this.Tagname.Name = "Tagname";
            this.Tagname.Size = new System.Drawing.Size(264, 20);
            this.Tagname.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tag name";
            // 
            // CreateTag
            // 
            this.CreateTag.Location = new System.Drawing.Point(626, 2);
            this.CreateTag.Name = "CreateTag";
            this.CreateTag.Size = new System.Drawing.Size(105, 23);
            this.CreateTag.TabIndex = 0;
            this.CreateTag.Text = "Create tag";
            this.CreateTag.UseVisualStyleBackColor = true;
            this.CreateTag.Click += new System.EventHandler(this.CreateTag_Click);
            // 
            // FormTag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 523);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormTag";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tag";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private RevisionGrid GitRevisions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CreateTag;
        private System.Windows.Forms.TextBox Tagname;
    }
}