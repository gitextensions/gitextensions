namespace GitUI.CommandsDialogs
{
    partial class FormDeleteTag
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
            this.Ok = new System.Windows.Forms.Button();
            this.deleteTag = new System.Windows.Forms.CheckBox();
            this.Tags = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.remotesComboboxControl1 = new GitUI.UserControls.RemotesComboboxControl();
            this.label2 = new System.Windows.Forms.Label();
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            this.label3 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Ok.Location = new System.Drawing.Point(369, 10);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 25);
            this.Ok.TabIndex = 8;
            this.Ok.Text = "Delete";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // deleteTag
            // 
            this.deleteTag.AutoSize = true;
            this.deleteTag.Location = new System.Drawing.Point(10, 81);
            this.deleteTag.Name = "deleteTag";
            this.deleteTag.Size = new System.Drawing.Size(262, 19);
            this.deleteTag.TabIndex = 11;
            this.deleteTag.Text = "Delete tag also from the following remote(s):";
            this.deleteTag.UseVisualStyleBackColor = true;
            this.deleteTag.CheckedChanged += new System.EventHandler(this.deleteTag_CheckedChanged);
            // 
            // Tags
            // 
            this.Tags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Tags.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Tags.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Tags.FormattingEnabled = true;
            this.Tags.Location = new System.Drawing.Point(126, 12);
            this.Tags.Name = "Tags";
            this.Tags.Size = new System.Drawing.Size(237, 23);
            this.Tags.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(7, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select tag";
            // 
            // remotesComboboxControl1
            // 
            this.remotesComboboxControl1.AllowMultiselect = false;
            this.remotesComboboxControl1.Location = new System.Drawing.Point(10, 106);
            this.remotesComboboxControl1.Name = "remotesComboboxControl1";
            this.remotesComboboxControl1.SelectedRemote = "";
            this.remotesComboboxControl1.Size = new System.Drawing.Size(270, 25);
            this.remotesComboboxControl1.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(315, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "This will delete the selected tag from the (local) repository.";
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.gotoUserManualControl1.AutoSize = true;
            this.gotoUserManualControl1.Location = new System.Drawing.Point(3, 3);
            this.gotoUserManualControl1.ManualSectionAnchorName = "delete-tag";
            this.gotoUserManualControl1.ManualSectionSubfolder = "tag";
            this.gotoUserManualControl1.MinimumSize = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(76, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(367, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "(includes information about deleting tags which are already pushed)";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.gotoUserManualControl1);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 147);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(454, 25);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // FormDeleteTag
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(454, 172);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.remotesComboboxControl1);
            this.Controls.Add(this.deleteTag);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Tags);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDeleteTag";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Delete tag";
            this.Load += new System.EventHandler(this.FormDeleteTagLoad);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.ComboBox Tags;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox deleteTag;
        private UserControls.RemotesComboboxControl remotesComboboxControl1;
        private System.Windows.Forms.Label label2;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}