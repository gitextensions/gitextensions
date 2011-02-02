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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.pnlAuthorColor = new System.Windows.Forms.Panel();
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
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthor.Location = new System.Drawing.Point(30, 8);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(46, 16);
            this.lblAuthor.TabIndex = 0;
            this.lblAuthor.Text = "Author";
            // 
            // pnlAuthorColor
            // 
            this.pnlAuthorColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAuthorColor.Location = new System.Drawing.Point(6, 6);
            this.pnlAuthorColor.Name = "pnlAuthorColor";
            this.pnlAuthorColor.Size = new System.Drawing.Size(20, 20);
            this.pnlAuthorColor.TabIndex = 1;
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 484);
            this.Controls.Add(this.Impact);
            this.Controls.Add(this.pnlTop);
            this.Name = "FormImpact";
            this.Text = "Impact";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ImpactControl Impact;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Panel pnlAuthorColor;
        private System.Windows.Forms.Label lblAuthor;
    }
}