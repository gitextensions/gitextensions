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
            if (disposing && (components is not null))
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
            TextLabel = new Label();
            ButtonContainer = new FlowLayoutPanel();
            ResolveButton = new Button();
            ContinueButton = new Button();
            AbortButton = new Button();
            MoreButton = new Button();
            IconBox = new PictureBox();
            ButtonContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(IconBox)).BeginInit();
            SuspendLayout();
            // 
            // TextLabel
            // 
            TextLabel.Dock = DockStyle.Fill;
            TextLabel.Location = new Point(28, 0);
            TextLabel.Name = "TextLabel";
            TextLabel.Size = new Size(396, 34);
            TextLabel.TabIndex = 0;
            TextLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ButtonContainer
            // 
            ButtonContainer.AutoSize = true;
            ButtonContainer.Controls.Add(ResolveButton);
            ButtonContainer.Controls.Add(ContinueButton);
            ButtonContainer.Controls.Add(AbortButton);
            ButtonContainer.Controls.Add(MoreButton);
            ButtonContainer.Dock = DockStyle.Right;
            ButtonContainer.Location = new Point(98, 0);
            ButtonContainer.MinimumSize = new Size(0, 33);
            ButtonContainer.Name = "ButtonContainer";
            ButtonContainer.Padding = new Padding(0, 2, 2, 2);
            ButtonContainer.Size = new Size(326, 34);
            ButtonContainer.TabIndex = 0;
            ButtonContainer.WrapContents = false;
            // 
            // ResolveButton
            // 
            ResolveButton.Location = new Point(3, 5);
            ResolveButton.Name = "ResolveButton";
            ResolveButton.Size = new Size(75, 23);
            ResolveButton.TabIndex = 6;
            ResolveButton.Text = "Resolve...";
            ResolveButton.UseVisualStyleBackColor = true;
            ResolveButton.Click += ResolveButton_Click;
            // 
            // ContinueButton
            // 
            ContinueButton.Location = new Point(84, 5);
            ContinueButton.Name = "ContinueButton";
            ContinueButton.Size = new Size(75, 23);
            ContinueButton.TabIndex = 1;
            ContinueButton.Text = "Continue";
            ContinueButton.UseVisualStyleBackColor = true;
            ContinueButton.Click += ContinueButton_Click;
            // 
            // AbortButton
            // 
            AbortButton.Location = new Point(165, 5);
            AbortButton.Name = "AbortButton";
            AbortButton.Size = new Size(75, 23);
            AbortButton.TabIndex = 2;
            AbortButton.Text = "Abort";
            AbortButton.UseVisualStyleBackColor = true;
            AbortButton.Click += AbortButton_Click;
            // 
            // MoreButton
            // 
            MoreButton.Location = new Point(246, 5);
            MoreButton.Name = "MoreButton";
            MoreButton.Size = new Size(75, 23);
            MoreButton.TabIndex = 5;
            MoreButton.Text = "M&ore...";
            MoreButton.UseVisualStyleBackColor = true;
            MoreButton.Click += MoreButton_Click;
            // 
            // IconBox
            // 
            IconBox.Dock = DockStyle.Left;
            IconBox.Image = Properties.Resources.information;
            IconBox.Location = new Point(0, 0);
            IconBox.Name = "IconBox";
            IconBox.Size = new Size(28, 34);
            IconBox.SizeMode = PictureBoxSizeMode.CenterImage;
            IconBox.TabIndex = 2;
            IconBox.TabStop = false;
            // 
            // InteractiveGitActionControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.LightSkyBlue;
            Controls.Add(ButtonContainer);
            Controls.Add(TextLabel);
            Controls.Add(IconBox);
            Name = "InteractiveGitActionControl";
            Size = new Size(424, 34);
            ButtonContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(IconBox)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label TextLabel;
        private FlowLayoutPanel ButtonContainer;
        private Button ContinueButton;
        private Button AbortButton;
        private Button MoreButton;
        private Button ResolveButton;
        private PictureBox IconBox;
    }
}
