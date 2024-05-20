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
            if (disposing && (components is not null))
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
            Start = new Button();
            Good = new Button();
            Bad = new Button();
            Stop = new Button();
            btnSkip = new Button();
            SuspendLayout();
            // 
            // Start
            // 
            Start.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Start.Location = new Point(12, 12);
            Start.Name = "Start";
            Start.Size = new Size(224, 25);
            Start.TabIndex = 0;
            Start.Text = "Start bisect";
            Start.UseVisualStyleBackColor = true;
            Start.Click += Start_Click;
            // 
            // Good
            // 
            Good.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Good.Location = new Point(12, 70);
            Good.Name = "Good";
            Good.Size = new Size(224, 25);
            Good.TabIndex = 2;
            Good.Text = "Mark current revision &good";
            Good.UseVisualStyleBackColor = true;
            Good.Click += Good_Click;
            // 
            // Bad
            // 
            Bad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Bad.Location = new Point(12, 41);
            Bad.Name = "Bad";
            Bad.Size = new Size(224, 25);
            Bad.TabIndex = 1;
            Bad.Text = "Mark current revision &bad";
            Bad.UseVisualStyleBackColor = true;
            Bad.Click += Bad_Click;
            // 
            // Stop
            // 
            Stop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Stop.Location = new Point(12, 132);
            Stop.Margin = new Padding(3, 3, 3, 12);
            Stop.Name = "Stop";
            Stop.Size = new Size(224, 25);
            Stop.TabIndex = 4;
            Stop.Text = "Stop bisect";
            Stop.UseVisualStyleBackColor = true;
            Stop.Click += Stop_Click;
            // 
            // btnSkip
            // 
            btnSkip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSkip.Location = new Point(12, 101);
            btnSkip.Name = "btnSkip";
            btnSkip.Size = new Size(224, 25);
            btnSkip.TabIndex = 3;
            btnSkip.Text = "&Skip current revision";
            btnSkip.UseVisualStyleBackColor = true;
            btnSkip.Click += btnSkip_Click;
            // 
            // FormBisect
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(248, 171);
            Controls.Add(btnSkip);
            Controls.Add(Stop);
            Controls.Add(Bad);
            Controls.Add(Good);
            Controls.Add(Start);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormBisect";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Bisect";
            ResumeLayout(false);

        }

        #endregion

        private Button Start;
        private Button Good;
        private Button Bad;
        private Button Stop;
        private Button btnSkip;
    }
}