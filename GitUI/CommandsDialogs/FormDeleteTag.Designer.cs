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
            Ok = new Button();
            deleteTag = new CheckBox();
            Tags = new ComboBox();
            label1 = new Label();
            remotesComboboxControl1 = new GitUI.UserControls.RemotesComboboxControl();
            label2 = new Label();
            gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            label3 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Ok.ForeColor = SystemColors.ControlText;
            Ok.Location = new Point(369, 10);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 25);
            Ok.TabIndex = 8;
            Ok.Text = "Delete";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // deleteTag
            // 
            deleteTag.AutoSize = true;
            deleteTag.Location = new Point(10, 81);
            deleteTag.Name = "deleteTag";
            deleteTag.Size = new Size(262, 19);
            deleteTag.TabIndex = 11;
            deleteTag.Text = "Delete tag also from the following remote(s):";
            deleteTag.UseVisualStyleBackColor = true;
            deleteTag.CheckedChanged += deleteTag_CheckedChanged;
            // 
            // Tags
            // 
            Tags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Tags.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Tags.AutoCompleteSource = AutoCompleteSource.ListItems;
            Tags.FormattingEnabled = true;
            Tags.Location = new Point(126, 12);
            Tags.Name = "Tags";
            Tags.Size = new Size(237, 23);
            Tags.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(7, 15);
            label1.Name = "label1";
            label1.Size = new Size(58, 15);
            label1.TabIndex = 6;
            label1.Text = "Select tag";
            // 
            // remotesComboboxControl1
            // 
            remotesComboboxControl1.AllowMultiselect = false;
            remotesComboboxControl1.Location = new Point(10, 106);
            remotesComboboxControl1.Name = "remotesComboboxControl1";
            remotesComboboxControl1.SelectedRemote = "";
            remotesComboboxControl1.Size = new Size(270, 25);
            remotesComboboxControl1.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(7, 52);
            label2.Name = "label2";
            label2.Size = new Size(315, 15);
            label2.TabIndex = 13;
            label2.Text = "This will delete the selected tag from the (local) repository.";
            // 
            // gotoUserManualControl1
            // 
            gotoUserManualControl1.Anchor = AnchorStyles.Left;
            gotoUserManualControl1.AutoSize = true;
            gotoUserManualControl1.Location = new Point(3, 3);
            gotoUserManualControl1.ManualSectionAnchorName = "delete-tag";
            gotoUserManualControl1.ManualSectionSubfolder = "tag";
            gotoUserManualControl1.MinimumSize = new Size(70, 20);
            gotoUserManualControl1.Name = "gotoUserManualControl1";
            gotoUserManualControl1.Size = new Size(70, 20);
            gotoUserManualControl1.TabIndex = 14;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(76, 5);
            label3.Margin = new Padding(0);
            label3.Name = "label3";
            label3.Size = new Size(367, 15);
            label3.TabIndex = 15;
            label3.Text = "(includes information about deleting tags which are already pushed)";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(gotoUserManualControl1);
            flowLayoutPanel1.Controls.Add(label3);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.Location = new Point(0, 147);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(454, 25);
            flowLayoutPanel1.TabIndex = 16;
            // 
            // FormDeleteTag
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(454, 172);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(label2);
            Controls.Add(remotesComboboxControl1);
            Controls.Add(deleteTag);
            Controls.Add(Ok);
            Controls.Add(Tags);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormDeleteTag";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Delete tag";
            Load += FormDeleteTagLoad;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button Ok;
        private ComboBox Tags;
        private Label label1;
        private CheckBox deleteTag;
        private UserControls.RemotesComboboxControl remotesComboboxControl1;
        private Label label2;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private Label label3;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}