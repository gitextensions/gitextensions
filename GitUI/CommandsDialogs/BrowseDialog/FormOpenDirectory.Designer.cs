﻿namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormOpenDirectory
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Directory = new System.Windows.Forms.ComboBox();
            this.Load = new System.Windows.Forms.Button();
            this.folderBrowserButton = new System.Windows.Forms.Button();
            this.folderGoUpButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Directory:";
            // 
            // _NO_TRANSLATE_Directory
            // 
            this._NO_TRANSLATE_Directory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Directory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Directory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_Directory.FormattingEnabled = true;
            this._NO_TRANSLATE_Directory.Location = new System.Drawing.Point(85, 9);
            this._NO_TRANSLATE_Directory.Name = "_NO_TRANSLATE_Directory";
            this._NO_TRANSLATE_Directory.Size = new System.Drawing.Size(360, 21);
            this._NO_TRANSLATE_Directory.TabIndex = 1;
            this._NO_TRANSLATE_Directory.TextChanged += new System.EventHandler(this._NO_TRANSLATE_Directory_TextChanged);
            this._NO_TRANSLATE_Directory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DirectoryKeyPress);
            // 
            // Load
            // 
            this.Load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Load.Image = global::GitUI.Properties.Images.RepoOpen;
            this.Load.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Load.Location = new System.Drawing.Point(448, 39);
            this.Load.Name = "Load";
            this.Load.Size = new System.Drawing.Size(164, 25);
            this.Load.TabIndex = 3;
            this.Load.Text = "Open";
            this.Load.UseVisualStyleBackColor = true;
            this.Load.Click += new System.EventHandler(this.LoadClick);
            // 
            // folderBrowserButton
            // 
            this.folderBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButton.AutoSize = true;
            this.folderBrowserButton.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.folderBrowserButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.folderBrowserButton.Location = new System.Drawing.Point(477, 7);
            this.folderBrowserButton.MinimumSize = new System.Drawing.Size(135, 25);
            this.folderBrowserButton.Name = "folderBrowserButton";
            this.folderBrowserButton.Size = new System.Drawing.Size(135, 25);
            this.folderBrowserButton.TabIndex = 4;
            this.folderBrowserButton.Text = "&Browse...";
            this.folderBrowserButton.UseVisualStyleBackColor = true;
            this.folderBrowserButton.Click += new System.EventHandler(this.folderBrowserButton_Click);
            // 
            // folderGoUpButton
            // 
            this.folderGoUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderGoUpButton.Image = global::GitUI.Properties.Images.NavigateUp;
            this.folderGoUpButton.Location = new System.Drawing.Point(448, 7);
            this.folderGoUpButton.Name = "folderGoUpButton";
            this.folderGoUpButton.Size = new System.Drawing.Size(26, 25);
            this.folderGoUpButton.TabIndex = 5;
            this.toolTip1.SetToolTip(this.folderGoUpButton, "Go to parent directory...");
            this.folderGoUpButton.UseVisualStyleBackColor = true;
            this.folderGoUpButton.Click += new System.EventHandler(this.folderGoUpButton_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 100;
            this.toolTip1.ShowAlways = true;
            this.toolTip1.ToolTipTitle = "Help";
            // 
            // FormOpenDirectory
            // 
            this.AcceptButton = this.Load;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(615, 81);
            this.Controls.Add(this.folderGoUpButton);
            this.Controls.Add(this.folderBrowserButton);
            this.Controls.Add(this.Load);
            this.Controls.Add(this._NO_TRANSLATE_Directory);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOpenDirectory";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open local repository";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Directory;
        private new System.Windows.Forms.Button Load;
        private System.Windows.Forms.Button folderBrowserButton;
        private System.Windows.Forms.Button folderGoUpButton;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
