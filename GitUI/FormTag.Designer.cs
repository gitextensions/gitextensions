using GitUI.SpellChecker;

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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.GitRevisions = new GitUI.RevisionGrid();
            this.label2 = new System.Windows.Forms.Label();
            this.tagMessage = new EditNetSpell();
            this.annotate = new System.Windows.Forms.CheckBox();
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
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.tagMessage);
            this.splitContainer1.Panel2.Controls.Add(this.annotate);
            this.splitContainer1.Panel2.Controls.Add(this.Tagname);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.CreateTag);
            this.splitContainer1.Size = new System.Drawing.Size(734, 523);
            this.splitContainer1.SplitterDistance = 352;
            this.splitContainer1.TabIndex = 0;
            // 
            // GitRevisions
            // 
            this.GitRevisions.CurrentCheckout = null;
            this.GitRevisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitRevisions.Filter = "";
            this.GitRevisions.LastRow = 0;
            this.GitRevisions.Location = new System.Drawing.Point(0, 0);
            this.GitRevisions.Name = "GitRevisions";
            this.GitRevisions.Size = new System.Drawing.Size(734, 352);
            this.GitRevisions.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Message";
            // 
            // tagMessage
            // 
            this.tagMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tagMessage.Enabled = false;
            this.tagMessage.Location = new System.Drawing.Point(110, 56);
            this.tagMessage.Name = "tagMessage";
            this.tagMessage.Size = new System.Drawing.Size(612, 99);
            this.tagMessage.TabIndex = 13;
            // 
            // annotate
            // 
            this.annotate.AutoSize = true;
            this.annotate.Location = new System.Drawing.Point(110, 33);
            this.annotate.Name = "annotate";
            this.annotate.Size = new System.Drawing.Size(126, 17);
            this.annotate.TabIndex = 12;
            this.annotate.Text = "Create annotated tag";
            this.annotate.UseVisualStyleBackColor = true;
            this.annotate.CheckedChanged += new System.EventHandler(this.AnnotateCheckedChanged);
            // 
            // Tagname
            // 
            this.Tagname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Tagname.Location = new System.Drawing.Point(110, 7);
            this.Tagname.Name = "Tagname";
            this.Tagname.Size = new System.Drawing.Size(457, 20);
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
            this.CreateTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateTag.Location = new System.Drawing.Point(626, 2);
            this.CreateTag.Name = "CreateTag";
            this.CreateTag.Size = new System.Drawing.Size(105, 23);
            this.CreateTag.TabIndex = 0;
            this.CreateTag.Text = "Create tag";
            this.CreateTag.UseVisualStyleBackColor = true;
            this.CreateTag.Click += new System.EventHandler(this.CreateTagClick);
            // 
            // FormTag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 523);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormTag";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tag";
            this.Load += new System.EventHandler(this.FormTagLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTagFormClosing);
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
        private System.Windows.Forms.Label label2;
        private EditNetSpell tagMessage;
        private System.Windows.Forms.CheckBox annotate;
    }
}