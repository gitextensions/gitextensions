namespace GitUI.CommandsDialogs
{
    partial class FormSvnClone
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSvnClone));
            this._NO_TRANSLATE_SvnFrom = new System.Windows.Forms.ComboBox();
            this._NO_TRANSLATE_destinationComboBox = new System.Windows.Forms.ComboBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_subdirectoryTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_authorsFileTextBox = new System.Windows.Forms.TextBox();
            this.authorsFileBrowseButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbTrunk = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.tbFrom = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_tbBranches = new System.Windows.Forms.TextBox();
            this.cbBranches = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_tbTags = new System.Windows.Forms.TextBox();
            this.cbTags = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_tbTrunk = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_SvnFrom
            // 
            this._NO_TRANSLATE_SvnFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_SvnFrom.FormattingEnabled = true;
            this._NO_TRANSLATE_SvnFrom.Location = new System.Drawing.Point(200, 16);
            this._NO_TRANSLATE_SvnFrom.Margin = new System.Windows.Forms.Padding(4);
            this._NO_TRANSLATE_SvnFrom.Name = "_NO_TRANSLATE_SvnFrom";
            this._NO_TRANSLATE_SvnFrom.Size = new System.Drawing.Size(441, 31);
            this._NO_TRANSLATE_SvnFrom.TabIndex = 1;
            this._NO_TRANSLATE_SvnFrom.SelectedIndexChanged += new System.EventHandler(this._NO_TRANSLATE_svnRepositoryComboBox_TextUpdate);
            this._NO_TRANSLATE_SvnFrom.TextUpdate += new System.EventHandler(this._NO_TRANSLATE_svnRepositoryComboBox_TextUpdate);
            // 
            // _NO_TRANSLATE_destinationComboBox
            // 
            this._NO_TRANSLATE_destinationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_destinationComboBox.FormattingEnabled = true;
            this._NO_TRANSLATE_destinationComboBox.Location = new System.Drawing.Point(200, 51);
            this._NO_TRANSLATE_destinationComboBox.Margin = new System.Windows.Forms.Padding(4);
            this._NO_TRANSLATE_destinationComboBox.Name = "_NO_TRANSLATE_destinationComboBox";
            this._NO_TRANSLATE_destinationComboBox.Size = new System.Drawing.Size(312, 31);
            this._NO_TRANSLATE_destinationComboBox.TabIndex = 2;
            this._NO_TRANSLATE_destinationComboBox.DropDown += new System.EventHandler(this.destinationComboBox_DropDown);
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(521, 50);
            this.browseButton.Margin = new System.Windows.Forms.Padding(4);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(121, 31);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(185, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Svn repository to clone";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 55);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Destination";
            // 
            // _NO_TRANSLATE_subdirectoryTextBox
            // 
            this._NO_TRANSLATE_subdirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_subdirectoryTextBox.Location = new System.Drawing.Point(200, 86);
            this._NO_TRANSLATE_subdirectoryTextBox.Margin = new System.Windows.Forms.Padding(4);
            this._NO_TRANSLATE_subdirectoryTextBox.Name = "_NO_TRANSLATE_subdirectoryTextBox";
            this._NO_TRANSLATE_subdirectoryTextBox.Size = new System.Drawing.Size(312, 30);
            this._NO_TRANSLATE_subdirectoryTextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 90);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(179, 23);
            this.label3.TabIndex = 7;
            this.label3.Text = "Subdirectory to create";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(524, 279);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(121, 31);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "Clone";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // _NO_TRANSLATE_authorsFileTextBox
            // 
            this._NO_TRANSLATE_authorsFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_authorsFileTextBox.Location = new System.Drawing.Point(200, 121);
            this._NO_TRANSLATE_authorsFileTextBox.Margin = new System.Windows.Forms.Padding(4);
            this._NO_TRANSLATE_authorsFileTextBox.Name = "_NO_TRANSLATE_authorsFileTextBox";
            this._NO_TRANSLATE_authorsFileTextBox.Size = new System.Drawing.Size(312, 30);
            this._NO_TRANSLATE_authorsFileTextBox.TabIndex = 8;
            // 
            // authorsFileBrowseButton
            // 
            this.authorsFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.authorsFileBrowseButton.Location = new System.Drawing.Point(521, 119);
            this.authorsFileBrowseButton.Margin = new System.Windows.Forms.Padding(4);
            this.authorsFileBrowseButton.Name = "authorsFileBrowseButton";
            this.authorsFileBrowseButton.Size = new System.Drawing.Size(121, 31);
            this.authorsFileBrowseButton.TabIndex = 9;
            this.authorsFileBrowseButton.Text = "Browse";
            this.authorsFileBrowseButton.UseVisualStyleBackColor = true;
            this.authorsFileBrowseButton.Click += new System.EventHandler(this.authorsFileBrowseButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 125);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 23);
            this.label4.TabIndex = 10;
            this.label4.Text = "Authors file";
            // 
            // cbTrunk
            // 
            this.cbTrunk.AutoSize = true;
            this.cbTrunk.Checked = true;
            this.cbTrunk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbTrunk.Location = new System.Drawing.Point(7, 29);
            this.cbTrunk.Name = "cbTrunk";
            this.cbTrunk.Size = new System.Drawing.Size(79, 27);
            this.cbTrunk.TabIndex = 11;
            this.cbTrunk.Text = "Trunk:";
            this.cbTrunk.UseVisualStyleBackColor = true;
            this.cbTrunk.CheckedChanged += new System.EventHandler(this.cbTrunk_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tbUsername);
            this.groupBox1.Controls.Add(this.tbFrom);
            this.groupBox1.Controls.Add(this._NO_TRANSLATE_tbBranches);
            this.groupBox1.Controls.Add(this.cbBranches);
            this.groupBox1.Controls.Add(this._NO_TRANSLATE_tbTags);
            this.groupBox1.Controls.Add(this.cbTags);
            this.groupBox1.Controls.Add(this._NO_TRANSLATE_tbTrunk);
            this.groupBox1.Controls.Add(this.cbTrunk);
            this.groupBox1.Location = new System.Drawing.Point(12, 167);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(633, 100);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Advanced settings";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(410, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 23);
            this.label6.TabIndex = 22;
            this.label6.Text = "Username";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 23);
            this.label5.TabIndex = 21;
            this.label5.Text = "From:";
            // 
            // tbUsername
            // 
            this.tbUsername.Enabled = false;
            this.tbUsername.Location = new System.Drawing.Point(521, 63);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(100, 30);
            this.tbUsername.TabIndex = 20;
            // 
            // tbFrom
            // 
            this.tbFrom.Enabled = false;
            this.tbFrom.Location = new System.Drawing.Point(92, 63);
            this.tbFrom.Name = "tbFrom";
            this.tbFrom.Size = new System.Drawing.Size(100, 30);
            this.tbFrom.TabIndex = 18;
            this.tbFrom.Text = "0";
            this.tbFrom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbFrom_KeyPress);
            // 
            // _NO_TRANSLATE_tbBranches
            // 
            this._NO_TRANSLATE_tbBranches.Location = new System.Drawing.Point(521, 27);
            this._NO_TRANSLATE_tbBranches.Name = "_NO_TRANSLATE_tbBranches";
            this._NO_TRANSLATE_tbBranches.Size = new System.Drawing.Size(100, 30);
            this._NO_TRANSLATE_tbBranches.TabIndex = 16;
            this._NO_TRANSLATE_tbBranches.Text = "branches";
            // 
            // cbBranches
            // 
            this.cbBranches.AutoSize = true;
            this.cbBranches.Checked = true;
            this.cbBranches.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBranches.Location = new System.Drawing.Point(407, 29);
            this.cbBranches.Name = "cbBranches";
            this.cbBranches.Size = new System.Drawing.Size(105, 27);
            this.cbBranches.TabIndex = 15;
            this.cbBranches.Text = "Branches:";
            this.cbBranches.UseVisualStyleBackColor = true;
            this.cbBranches.CheckedChanged += new System.EventHandler(this.cbBranches_CheckedChanged);
            // 
            // _NO_TRANSLATE_tbTags
            // 
            this._NO_TRANSLATE_tbTags.Location = new System.Drawing.Point(287, 27);
            this._NO_TRANSLATE_tbTags.Name = "_NO_TRANSLATE_tbTags";
            this._NO_TRANSLATE_tbTags.Size = new System.Drawing.Size(100, 30);
            this._NO_TRANSLATE_tbTags.TabIndex = 14;
            this._NO_TRANSLATE_tbTags.Text = "tags";
            // 
            // cbTags
            // 
            this.cbTags.AutoSize = true;
            this.cbTags.Checked = true;
            this.cbTags.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbTags.Location = new System.Drawing.Point(210, 29);
            this.cbTags.Name = "cbTags";
            this.cbTags.Size = new System.Drawing.Size(71, 27);
            this.cbTags.TabIndex = 13;
            this.cbTags.Text = "Tags:";
            this.cbTags.UseVisualStyleBackColor = true;
            this.cbTags.CheckedChanged += new System.EventHandler(this.cbTags_CheckedChanged);
            // 
            // _NO_TRANSLATE_tbTrunk
            // 
            this._NO_TRANSLATE_tbTrunk.Location = new System.Drawing.Point(92, 27);
            this._NO_TRANSLATE_tbTrunk.Name = "_NO_TRANSLATE_tbTrunk";
            this._NO_TRANSLATE_tbTrunk.Size = new System.Drawing.Size(100, 30);
            this._NO_TRANSLATE_tbTrunk.TabIndex = 12;
            this._NO_TRANSLATE_tbTrunk.Text = "trunk";
            // 
            // FormSvnClone
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(657, 323);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._NO_TRANSLATE_authorsFileTextBox);
            this.Controls.Add(this._NO_TRANSLATE_subdirectoryTextBox);
            this.Controls.Add(this._NO_TRANSLATE_destinationComboBox);
            this.Controls.Add(this._NO_TRANSLATE_SvnFrom);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.authorsFileBrowseButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.browseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(870, 370);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(675, 370);
            this.Name = "FormSvnClone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Svn Clone";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _NO_TRANSLATE_SvnFrom;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_destinationComboBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_subdirectoryTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_authorsFileTextBox;
        private System.Windows.Forms.Button authorsFileBrowseButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbTrunk;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.TextBox tbFrom;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_tbBranches;
        private System.Windows.Forms.CheckBox cbBranches;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_tbTags;
        private System.Windows.Forms.CheckBox cbTags;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_tbTrunk;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
    }
}