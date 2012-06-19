using GitUI.SpellChecker;

namespace GitUI.Tag
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
            this.tagMessage = new GitUI.SpellChecker.EditNetSpell();
            this.annotate = new System.Windows.Forms.CheckBox();
            this.Tagname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCreateTag = new System.Windows.Forms.Button();
            this.pushTag = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
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
            this.splitContainer1.Panel2.Controls.Add(this.btnCreateTag);
            this.splitContainer1.Panel2.Controls.Add(this.pushTag);
            this.splitContainer1.Size = new System.Drawing.Size(734, 523);
            this.splitContainer1.SplitterDistance = 352;
            this.splitContainer1.TabIndex = 0;
            // 
            // GitRevisions
            // 
            this.GitRevisions.BranchFilter = "";
            this.GitRevisions.CurrentCheckout = null;
            this.GitRevisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitRevisions.Filter = "";
            this.GitRevisions.FixedFilter = "";
            this.GitRevisions.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.GitRevisions.InMemAuthorFilter = "";
            this.GitRevisions.InMemCommitterFilter = "";
            this.GitRevisions.InMemMessageFilter = "";
            this.GitRevisions.LastRow = 0;
            this.GitRevisions.Location = new System.Drawing.Point(0, 0);
            this.GitRevisions.Name = "GitRevisions";
            this.GitRevisions.NormalFont = new System.Drawing.Font("Tahoma", 8.75F);
            this.GitRevisions.Size = new System.Drawing.Size(734, 352);
            this.GitRevisions.SuperprojectCurrentCheckout = null;
            this.GitRevisions.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "Message";
            // 
            // tagMessage
            // 
            this.tagMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagMessage.Enabled = false;
            this.tagMessage.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.tagMessage.Location = new System.Drawing.Point(110, 73);
            this.tagMessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tagMessage.MistakeFont = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Underline);
            this.tagMessage.Name = "tagMessage";
            this.tagMessage.Size = new System.Drawing.Size(612, 82);
            this.tagMessage.TabIndex = 13;
            this.tagMessage.WatermarkText = "";
            // 
            // annotate
            // 
            this.annotate.AutoSize = true;
            this.annotate.Location = new System.Drawing.Point(110, 52);
            this.annotate.Name = "annotate";
            this.annotate.Size = new System.Drawing.Size(116, 16);
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
            this.Tagname.Size = new System.Drawing.Size(457, 21);
            this.Tagname.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tag name";
            // 
            // btnCreateTag
            // 
            this.btnCreateTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateTag.Location = new System.Drawing.Point(626, 2);
            this.btnCreateTag.Name = "btnCreateTag";
            this.btnCreateTag.Size = new System.Drawing.Size(105, 23);
            this.btnCreateTag.TabIndex = 0;
            this.btnCreateTag.Text = "Create tag";
            this.btnCreateTag.UseVisualStyleBackColor = true;
            this.btnCreateTag.Click += new System.EventHandler(this.CreateTagClick);
            // 
            // pushTag
            // 
            this.pushTag.AutoSize = true;
            this.pushTag.Location = new System.Drawing.Point(110, 33);
            this.pushTag.Name = "pushTag";
            this.pushTag.Size = new System.Drawing.Size(63, 16);
            this.pushTag.TabIndex = 12;
            this.pushTag.Text = "Push tag";
            this.pushTag.UseVisualStyleBackColor = true;
            // 
            // FormTag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 523);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormTag";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tag";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTagFormClosing);
            this.Load += new System.EventHandler(this.FormTagLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private RevisionGrid GitRevisions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCreateTag;
        private System.Windows.Forms.CheckBox pushTag;
        private System.Windows.Forms.TextBox Tagname;
        private System.Windows.Forms.Label label2;
        private EditNetSpell tagMessage;
        private System.Windows.Forms.CheckBox annotate;
    }
}