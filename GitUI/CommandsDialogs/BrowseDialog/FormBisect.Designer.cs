namespace GitUI.CommandsDialogs.BrowseDialog
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
            this.btnSkip = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Start
            // 
            this.Start.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Start.Location = new System.Drawing.Point(15, 15);
            this.Start.Margin = new System.Windows.Forms.Padding(4);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(280, 31);
            this.Start.TabIndex = 0;
            this.Start.Text = "Start bisect";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Good
            // 
            this.Good.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Good.Location = new System.Drawing.Point(15, 88);
            this.Good.Margin = new System.Windows.Forms.Padding(4);
            this.Good.Name = "Good";
            this.Good.Size = new System.Drawing.Size(280, 31);
            this.Good.TabIndex = 1;
            this.Good.Text = "Mark current revision good";
            this.Good.UseVisualStyleBackColor = true;
            this.Good.Click += new System.EventHandler(this.Good_Click);
            // 
            // Bad
            // 
            this.Bad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Bad.Location = new System.Drawing.Point(15, 51);
            this.Bad.Margin = new System.Windows.Forms.Padding(4);
            this.Bad.Name = "Bad";
            this.Bad.Size = new System.Drawing.Size(280, 31);
            this.Bad.TabIndex = 2;
            this.Bad.Text = "Mark current revision bad";
            this.Bad.UseVisualStyleBackColor = true;
            this.Bad.Click += new System.EventHandler(this.Bad_Click);
            // 
            // Stop
            // 
            this.Stop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Stop.Location = new System.Drawing.Point(15, 165);
            this.Stop.Margin = new System.Windows.Forms.Padding(4);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(280, 31);
            this.Stop.TabIndex = 3;
            this.Stop.Text = "Stop bisect";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // btnSkip
            // 
            this.btnSkip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSkip.Location = new System.Drawing.Point(15, 126);
            this.btnSkip.Margin = new System.Windows.Forms.Padding(4);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(280, 31);
            this.btnSkip.TabIndex = 4;
            this.btnSkip.Text = "Skip current revision";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.btnSkip_Click);
            // 
            // FormBisect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(310, 208);
            this.Controls.Add(this.btnSkip);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.Bad);
            this.Controls.Add(this.Good);
            this.Controls.Add(this.Start);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 253);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(253, 253);
            this.Name = "FormBisect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bisect";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button Good;
        private System.Windows.Forms.Button Bad;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button btnSkip;
    }
}