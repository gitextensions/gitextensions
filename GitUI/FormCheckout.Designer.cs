namespace GitUI
{
    partial class FormCheckout
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
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.OutPut = new System.Windows.Forms.RichTextBox();
            this.Branch = new System.Windows.Forms.ComboBox();
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
            this.splitContainer1.Panel1.Controls.Add(this.Branch);
            this.splitContainer1.Panel1.Controls.Add(this.Ok);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.OutPut);
            this.splitContainer1.Size = new System.Drawing.Size(470, 286);
            this.splitContainer1.SplitterDistance = 49;
            this.splitContainer1.TabIndex = 0;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(383, 12);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 20);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "Checkout";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name";
            // 
            // OutPut
            // 
            this.OutPut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutPut.Location = new System.Drawing.Point(0, 0);
            this.OutPut.Name = "OutPut";
            this.OutPut.Size = new System.Drawing.Size(470, 233);
            this.OutPut.TabIndex = 0;
            this.OutPut.Text = "";
            // 
            // Branch
            // 
            this.Branch.FormattingEnabled = true;
            this.Branch.Location = new System.Drawing.Point(54, 13);
            this.Branch.Name = "Branch";
            this.Branch.Size = new System.Drawing.Size(265, 21);
            this.Branch.TabIndex = 3;
            this.Branch.SelectedIndexChanged += new System.EventHandler(this.Branch_SelectedIndexChanged);
            // 
            // FormCheckout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 286);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormCheckout";
            this.Text = "FormCheckout";
            this.Load += new System.EventHandler(this.FormCheckout_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox OutPut;
        private System.Windows.Forms.ComboBox Branch;
    }
}