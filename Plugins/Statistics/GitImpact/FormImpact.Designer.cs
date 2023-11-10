namespace GitExtensions.Plugins.GitImpact
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImpact));
            pnlTop = new Panel();
            pnlAuthorColor = new Panel();
            lblAuthor = new Label();
            cbIncludingSubmodules = new CheckBox();
            Impact = new GitImpact.ImpactControl();
            pnlTop.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(pnlAuthorColor);
            pnlTop.Controls.Add(lblAuthor);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(863, 32);
            pnlTop.TabIndex = 1;
            // 
            // pnlAuthorColor
            // 
            pnlAuthorColor.BorderStyle = BorderStyle.FixedSingle;
            pnlAuthorColor.Location = new Point(6, 6);
            pnlAuthorColor.Name = "pnlAuthorColor";
            pnlAuthorColor.Size = new Size(20, 20);
            pnlAuthorColor.TabIndex = 1;
            // 
            // lblAuthor
            // 
            lblAuthor.AutoSize = true;
            lblAuthor.Location = new Point(30, 8);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(46, 16);
            lblAuthor.TabIndex = 0;
            lblAuthor.Text = "Author";
            // 
            // cbIncludingSubmodules
            // 
            cbIncludingSubmodules.AutoSize = true;
            cbIncludingSubmodules.Location = new Point(692, 9);
            cbIncludingSubmodules.Name = "cbIncludingSubmodules";
            cbIncludingSubmodules.Size = new Size(128, 17);
            cbIncludingSubmodules.TabIndex = 2;
            cbIncludingSubmodules.Text = "Including submodules";
            cbIncludingSubmodules.UseVisualStyleBackColor = true;
            cbIncludingSubmodules.CheckedChanged += cbShowSubmodules_CheckedChanged;
            // 
            // Impact
            // 
            Impact.BorderStyle = BorderStyle.Fixed3D;
            Impact.Dock = DockStyle.Fill;
            Impact.Location = new Point(0, 32);
            Impact.Name = "Impact";
            Impact.Size = new Size(863, 452);
            Impact.TabIndex = 0;
            Impact.TabStop = false;
            Impact.MouseMove += Impact_MouseMove;
            // 
            // FormImpact
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(863, 484);
            Controls.Add(cbIncludingSubmodules);
            Controls.Add(Impact);
            Controls.Add(pnlTop);
            Name = "FormImpact";
            Text = "Impact";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ImpactControl Impact;
        private Panel pnlTop;
        private Panel pnlAuthorColor;
        private Label lblAuthor;
        private CheckBox cbIncludingSubmodules;
    }
}
