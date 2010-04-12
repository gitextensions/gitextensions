namespace GitUI
{
    partial class CommitInfo
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.RevisionInfo = new System.Windows.Forms.RichTextBox();
            this.gravatar1 = new GitUI.Gravatar();
            this.tableLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayout
            // 
            this.tableLayout.ColumnCount = 2;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout.Controls.Add(this.gravatar1, 0, 0);
            this.tableLayout.Controls.Add(this.RevisionInfo, 1, 0);
            this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayout.Location = new System.Drawing.Point(0, 0);
            this.tableLayout.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.RowCount = 1;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout.Size = new System.Drawing.Size(766, 334);
            this.tableLayout.TabIndex = 3;
            // 
            // RevisionInfo
            // 
            this.RevisionInfo.BackColor = System.Drawing.SystemColors.Window;
            this.RevisionInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RevisionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionInfo.Location = new System.Drawing.Point(90, 0);
            this.RevisionInfo.Margin = new System.Windows.Forms.Padding(0);
            this.RevisionInfo.Name = "RevisionInfo";
            this.RevisionInfo.ReadOnly = true;
            this.RevisionInfo.Size = new System.Drawing.Size(676, 334);
            this.RevisionInfo.TabIndex = 0;
            this.RevisionInfo.Text = "";
            // 
            // gravatar1
            // 
            this.gravatar1.BackColor = System.Drawing.SystemColors.Window;
            this.gravatar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gravatar1.email = null;
            this.gravatar1.Location = new System.Drawing.Point(0, 0);
            this.gravatar1.Margin = new System.Windows.Forms.Padding(0);
            this.gravatar1.Name = "gravatar1";
            this.gravatar1.Size = new System.Drawing.Size(90, 334);
            this.gravatar1.TabIndex = 1;
            // 
            // CommitInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayout);
            this.Name = "CommitInfo";
            this.Size = new System.Drawing.Size(766, 334);
            this.tableLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private Gravatar gravatar1;
        private System.Windows.Forms.RichTextBox RevisionInfo;
    }
}
