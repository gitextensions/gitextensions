namespace GitUI
{
    partial class FormRevert
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRevert));
            this.RevertLabel = new System.Windows.Forms.Label();
            this.Revert = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RevertLabel
            // 
            this.RevertLabel.AutoSize = true;
            this.RevertLabel.Location = new System.Drawing.Point(13, 13);
            this.RevertLabel.Name = "RevertLabel";
            this.RevertLabel.Size = new System.Drawing.Size(35, 13);
            this.RevertLabel.TabIndex = 0;
            this.RevertLabel.Text = "label1";
            // 
            // Revert
            // 
            this.Revert.Location = new System.Drawing.Point(356, 38);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(91, 23);
            this.Revert.TabIndex = 1;
            this.Revert.Text = "Revert changes";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(261, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(89, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormRevert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 73);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Revert);
            this.Controls.Add(this.RevertLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormRevert";
            this.Text = "Revert file changes";
            this.Load += new System.EventHandler(this.FormRevert_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label RevertLabel;
        private System.Windows.Forms.Button Revert;
        private System.Windows.Forms.Button button1;
    }
}