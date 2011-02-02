namespace GitImpact
{
    partial class FormImpact
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
            this.Impact = new GitImpact.ImpactControl();
            this.SuspendLayout();
            // 
            // Impact
            // 
            this.Impact.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Impact.Location = new System.Drawing.Point(0, 0);
            this.Impact.Name = "Impact";
            this.Impact.Size = new System.Drawing.Size(863, 484);
            this.Impact.TabIndex = 0;
            this.Impact.TabStop = false;
            this.Impact.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Impact_MouseClick);
            this.Impact.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Impact_MouseMove);
            // 
            // FormImpact
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 484);
            this.Controls.Add(this.Impact);
            this.Name = "FormImpact";
            this.Text = "Impact";
            this.ResumeLayout(false);

        }

        #endregion

        private ImpactControl Impact;
    }
}