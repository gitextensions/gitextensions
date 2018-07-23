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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImpact));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.pnlAuthorColor = new System.Windows.Forms.Panel();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.cbIncludingSubmodules = new System.Windows.Forms.CheckBox();
            this.Impact = new GitImpact.ImpactControl();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.pnlAuthorColor);
            this.pnlTop.Controls.Add(this.lblAuthor);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(863, 32);
            this.pnlTop.TabIndex = 1;
            // 
            // pnlAuthorColor
            // 
            this.pnlAuthorColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAuthorColor.Location = new System.Drawing.Point(6, 6);
            this.pnlAuthorColor.Name = "pnlAuthorColor";
            this.pnlAuthorColor.Size = new System.Drawing.Size(20, 20);
            this.pnlAuthorColor.TabIndex = 1;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(30, 8);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(46, 16);
            this.lblAuthor.TabIndex = 0;
            this.lblAuthor.Text = "Author";
            // 
            // cbIncludingSubmodules
            // 
            this.cbIncludingSubmodules.AutoSize = true;
            this.cbIncludingSubmodules.Location = new System.Drawing.Point(692, 9);
            this.cbIncludingSubmodules.Name = "cbIncludingSubmodules";
            this.cbIncludingSubmodules.Size = new System.Drawing.Size(128, 17);
            this.cbIncludingSubmodules.TabIndex = 2;
            this.cbIncludingSubmodules.Text = "Including submodules";
            this.cbIncludingSubmodules.UseVisualStyleBackColor = true;
            this.cbIncludingSubmodules.CheckedChanged += new System.EventHandler(this.cbShowSubmodules_CheckedChanged);
            // 
            // Impact
            // 
            this.Impact.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Impact.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Impact.Location = new System.Drawing.Point(0, 32);
            this.Impact.Name = "Impact";
            this.Impact.Size = new System.Drawing.Size(863, 452);
            this.Impact.TabIndex = 0;
            this.Impact.TabStop = false;
            this.Impact.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Impact_MouseMove);
            // 
            // FormImpact
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(863, 484);
            this.Controls.Add(this.cbIncludingSubmodules);
            this.Controls.Add(this.Impact);
            this.Controls.Add(this.pnlTop);
            this.Name = "FormImpact";
            this.Text = "Impact";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ImpactControl Impact;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Panel pnlAuthorColor;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.CheckBox cbIncludingSubmodules;
    }
}