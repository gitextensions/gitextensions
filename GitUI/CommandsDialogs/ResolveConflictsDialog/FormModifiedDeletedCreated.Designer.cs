namespace GitUI.CommandsDialogs.ResolveConflictsDialog
{
    partial class FormModifiedDeletedCreated
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
            this.Label = new System.Windows.Forms.Label();
            this.Local = new System.Windows.Forms.Button();
            this.Remote = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Base = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.questionImage = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.questionImage)).BeginInit();
            this.SuspendLayout();
            // 
            // Label
            // 
            this.Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label.Location = new System.Drawing.Point(67, 0);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(420, 61);
            this.Label.TabIndex = 0;
            this.Label.Text = "Use created or deleted file?";
            // 
            // Local
            // 
            this.Local.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Local.Location = new System.Drawing.Point(0, 0);
            this.Local.Margin = new System.Windows.Forms.Padding(0);
            this.Local.Name = "Local";
            this.Local.Size = new System.Drawing.Size(163, 25);
            this.Local.TabIndex = 1;
            this.Local.Text = "Local";
            this.Local.UseVisualStyleBackColor = true;
            this.Local.Click += new System.EventHandler(this.Local_Click);
            // 
            // Remote
            // 
            this.Remote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remote.Location = new System.Drawing.Point(163, 0);
            this.Remote.Margin = new System.Windows.Forms.Padding(0);
            this.Remote.Name = "Remote";
            this.Remote.Size = new System.Drawing.Size(163, 25);
            this.Remote.TabIndex = 2;
            this.Remote.Text = "Remote";
            this.Remote.UseVisualStyleBackColor = true;
            this.Remote.Click += new System.EventHandler(this.Remote_Click);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(387, 72);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(112, 21);
            this.Abort.TabIndex = 3;
            this.Abort.TabStop = false;
            this.Abort.Text = "Abort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.AbortClick);
            // 
            // Base
            // 
            this.Base.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Base.Location = new System.Drawing.Point(326, 0);
            this.Base.Margin = new System.Windows.Forms.Padding(0);
            this.Base.Name = "Base";
            this.Base.Size = new System.Drawing.Size(164, 25);
            this.Base.TabIndex = 4;
            this.Base.Text = "Base";
            this.Base.UseVisualStyleBackColor = true;
            this.Base.Click += new System.EventHandler(this.Base_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(496, 98);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Controls.Add(this.Local, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Remote, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.Base, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 70);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(490, 25);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.Label, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.questionImage, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(490, 61);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // questionImage
            // 
            this.questionImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.questionImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.questionImage.Location = new System.Drawing.Point(0, 0);
            this.questionImage.Margin = new System.Windows.Forms.Padding(0);
            this.questionImage.Name = "questionImage";
            this.questionImage.Size = new System.Drawing.Size(64, 61);
            this.questionImage.TabIndex = 6;
            this.questionImage.TabStop = false;
            // 
            // FormModifiedDeletedCreated
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(496, 98);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.Abort);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormModifiedDeletedCreated";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Solve merge conflict";
            this.Load += new System.EventHandler(this.FormModifiedDeletedCreated_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.questionImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Remote;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Local;
        private System.Windows.Forms.Button Base;
        private System.Windows.Forms.Label Label;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.PictureBox questionImage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}