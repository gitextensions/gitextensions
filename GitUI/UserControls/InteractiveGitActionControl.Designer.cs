using System;

namespace GitUI.UserControls
{
    partial class InteractiveGitActionControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TextLabel = new System.Windows.Forms.Label();
            this.ButtonContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.ResolveButton = new System.Windows.Forms.Button();
            this.ContinueButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            this.MoreButton = new System.Windows.Forms.Button();
            this.IconBox = new System.Windows.Forms.PictureBox();
            this.ButtonContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // TextLabel
            // 
            this.TextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextLabel.Location = new System.Drawing.Point(28, 0);
            this.TextLabel.Name = "TextLabel";
            this.TextLabel.Size = new System.Drawing.Size(396, 34);
            this.TextLabel.TabIndex = 0;
            this.TextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ButtonContainer
            // 
            this.ButtonContainer.AutoSize = true;
            this.ButtonContainer.Controls.Add(this.ResolveButton);
            this.ButtonContainer.Controls.Add(this.ContinueButton);
            this.ButtonContainer.Controls.Add(this.AbortButton);
            this.ButtonContainer.Controls.Add(this.MoreButton);
            this.ButtonContainer.Dock = System.Windows.Forms.DockStyle.Right;
            this.ButtonContainer.Location = new System.Drawing.Point(98, 0);
            this.ButtonContainer.MinimumSize = new System.Drawing.Size(0, 33);
            this.ButtonContainer.Name = "ButtonContainer";
            this.ButtonContainer.Padding = new System.Windows.Forms.Padding(0, 2, 2, 2);
            this.ButtonContainer.Size = new System.Drawing.Size(326, 34);
            this.ButtonContainer.TabIndex = 0;
            this.ButtonContainer.WrapContents = false;
            // 
            // ResolveButton
            // 
            this.ResolveButton.Location = new System.Drawing.Point(3, 5);
            this.ResolveButton.Name = "ResolveButton";
            this.ResolveButton.Size = new System.Drawing.Size(75, 23);
            this.ResolveButton.TabIndex = 6;
            this.ResolveButton.Text = "Resolve...";
            this.ResolveButton.UseVisualStyleBackColor = true;
            this.ResolveButton.Click += new System.EventHandler(this.ResolveButton_Click);
            // 
            // ContinueButton
            // 
            this.ContinueButton.Location = new System.Drawing.Point(84, 5);
            this.ContinueButton.Name = "ContinueButton";
            this.ContinueButton.Size = new System.Drawing.Size(75, 23);
            this.ContinueButton.TabIndex = 1;
            this.ContinueButton.Text = "Continue";
            this.ContinueButton.UseVisualStyleBackColor = true;
            this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // AbortButton
            // 
            this.AbortButton.Location = new System.Drawing.Point(165, 5);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(75, 23);
            this.AbortButton.TabIndex = 2;
            this.AbortButton.Text = "Abort";
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            // 
            // MoreButton
            // 
            this.MoreButton.Location = new System.Drawing.Point(246, 5);
            this.MoreButton.Name = "MoreButton";
            this.MoreButton.Size = new System.Drawing.Size(75, 23);
            this.MoreButton.TabIndex = 5;
            this.MoreButton.Text = "M&ore...";
            this.MoreButton.UseVisualStyleBackColor = true;
            this.MoreButton.Click += new System.EventHandler(this.MoreButton_Click);
            // 
            // IconBox
            // 
            this.IconBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.IconBox.Image = global::GitUI.Properties.Resources.information;
            this.IconBox.Location = new System.Drawing.Point(0, 0);
            this.IconBox.Name = "IconBox";
            this.IconBox.Size = new System.Drawing.Size(28, 34);
            this.IconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.IconBox.TabIndex = 2;
            this.IconBox.TabStop = false;
            // 
            // InteractiveGitActionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.LightSkyBlue;
            this.Controls.Add(this.ButtonContainer);
            this.Controls.Add(this.TextLabel);
            this.Controls.Add(this.IconBox);
            this.Name = "InteractiveGitActionControl";
            this.Size = new System.Drawing.Size(424, 34);
            this.ButtonContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TextLabel;
        private System.Windows.Forms.FlowLayoutPanel ButtonContainer;
        private System.Windows.Forms.Button ContinueButton;
        private System.Windows.Forms.Button AbortButton;
        private System.Windows.Forms.Button MoreButton;
        private System.Windows.Forms.Button ResolveButton;
        private System.Windows.Forms.PictureBox IconBox;
    }
}
