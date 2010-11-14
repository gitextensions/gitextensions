namespace GitUI
{
    partial class FormBisect
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
            this.Start = new System.Windows.Forms.Button();
            this.Good = new System.Windows.Forms.Button();
            this.Bad = new System.Windows.Forms.Button();
            this.Stop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(12, 12);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(174, 23);
            this.Start.TabIndex = 0;
            this.Start.Text = "Start bisect";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Good
            // 
            this.Good.Location = new System.Drawing.Point(12, 70);
            this.Good.Name = "Good";
            this.Good.Size = new System.Drawing.Size(174, 23);
            this.Good.TabIndex = 1;
            this.Good.Text = "Mark current revision good";
            this.Good.UseVisualStyleBackColor = true;
            this.Good.Click += new System.EventHandler(this.Good_Click);
            // 
            // Bad
            // 
            this.Bad.Location = new System.Drawing.Point(12, 41);
            this.Bad.Name = "Bad";
            this.Bad.Size = new System.Drawing.Size(174, 23);
            this.Bad.TabIndex = 2;
            this.Bad.Text = "Mark current revision bad";
            this.Bad.UseVisualStyleBackColor = true;
            this.Bad.Click += new System.EventHandler(this.Bad_Click);
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(12, 99);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(174, 23);
            this.Stop.TabIndex = 3;
            this.Stop.Text = "Stop bisect";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // FormBisect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 134);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.Bad);
            this.Controls.Add(this.Good);
            this.Controls.Add(this.Start);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBisect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bisect";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button Good;
        private System.Windows.Forms.Button Bad;
        private System.Windows.Forms.Button Stop;
    }
}