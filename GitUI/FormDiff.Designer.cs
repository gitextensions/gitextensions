namespace GitUI
{
    partial class FormDiff
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.OutPut = new System.Windows.Forms.RichTextBox();
            this.From = new System.Windows.Forms.ComboBox();
            this.To = new System.Windows.Forms.ComboBox();
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
            this.splitContainer1.Panel1.Controls.Add(this.To);
            this.splitContainer1.Panel1.Controls.Add(this.From);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.OutPut);
            this.splitContainer1.Size = new System.Drawing.Size(628, 401);
            this.splitContainer1.SplitterDistance = 70;
            this.splitContainer1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "To";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "From";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(541, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Show";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // OutPut
            // 
            this.OutPut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutPut.Location = new System.Drawing.Point(0, 0);
            this.OutPut.Name = "OutPut";
            this.OutPut.Size = new System.Drawing.Size(628, 327);
            this.OutPut.TabIndex = 0;
            this.OutPut.Text = "";
            // 
            // From
            // 
            this.From.FormattingEnabled = true;
            this.From.Location = new System.Drawing.Point(74, 17);
            this.From.Name = "From";
            this.From.Size = new System.Drawing.Size(293, 21);
            this.From.TabIndex = 5;
            this.From.SelectedIndexChanged += new System.EventHandler(this.From_SelectedIndexChanged);
            // 
            // To
            // 
            this.To.FormattingEnabled = true;
            this.To.Location = new System.Drawing.Point(74, 45);
            this.To.Name = "To";
            this.To.Size = new System.Drawing.Size(293, 21);
            this.To.TabIndex = 6;
            // 
            // FormDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 401);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormDiff";
            this.Text = "FormDiff";
            this.Load += new System.EventHandler(this.FormDiff_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox OutPut;
        private System.Windows.Forms.ComboBox To;
        private System.Windows.Forms.ComboBox From;
    }
}