namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class FormBrowseRepoSettingsPage
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
            this.chkShowGpgInformation = new System.Windows.Forms.CheckBox();
            this.chkChowConsoleTab = new System.Windows.Forms.CheckBox();
            this.cboTerminal = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkUseBrowseForFileHistory = new System.Windows.Forms.CheckBox();
            this.chkUseDiffViewerForBlame = new System.Windows.Forms.CheckBox();
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.gbTabs = new System.Windows.Forms.GroupBox();
            this.gbGeneral.SuspendLayout();
            this.gbTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkShowGpgInformation
            // 
            this.chkShowGpgInformation.AutoSize = true;
            this.chkShowGpgInformation.Location = new System.Drawing.Point(9, 48);
            this.chkShowGpgInformation.Name = "chkShowGpgInformation";
            this.chkShowGpgInformation.Size = new System.Drawing.Size(147, 19);
            this.chkShowGpgInformation.TabIndex = 7;
            this.chkShowGpgInformation.Text = "Show GPG information";
            this.chkShowGpgInformation.UseVisualStyleBackColor = true;
            // 
            // chkChowConsoleTab
            // 
            this.chkChowConsoleTab.AutoSize = true;
            this.chkChowConsoleTab.Location = new System.Drawing.Point(9, 25);
            this.chkChowConsoleTab.Name = "chkChowConsoleTab";
            this.chkChowConsoleTab.Size = new System.Drawing.Size(141, 19);
            this.chkChowConsoleTab.TabIndex = 6;
            this.chkChowConsoleTab.Text = "Show the Console tab";
            this.chkChowConsoleTab.UseVisualStyleBackColor = true;
            // 
            // cboTerminal
            // 
            this.cboTerminal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTerminal.FormattingEnabled = true;
            this.cboTerminal.Location = new System.Drawing.Point(146, 22);
            this.cboTerminal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboTerminal.Name = "cboTerminal";
            this.cboTerminal.Size = new System.Drawing.Size(262, 23);
            this.cboTerminal.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Default shell";
            // 
            // chkUseBrowseForFileHistory
            // 
            this.chkUseBrowseForFileHistory.AutoSize = true;
            this.chkUseBrowseForFileHistory.Location = new System.Drawing.Point(9, 48);
            this.chkUseBrowseForFileHistory.Name = "chkUseBrowseForFileHistory";
            this.chkUseBrowseForFileHistory.Size = new System.Drawing.Size(165, 19);
            this.chkUseBrowseForFileHistory.TabIndex = 4;
            this.chkUseBrowseForFileHistory.Text = "Show file history in the main window";
            this.chkUseBrowseForFileHistory.UseVisualStyleBackColor = true;
            // 
            // chkUseDiffViewerForBlame
            // 
            this.chkUseDiffViewerForBlame.AutoSize = true;
            this.chkUseDiffViewerForBlame.Location = new System.Drawing.Point(9, 71);
            this.chkUseDiffViewerForBlame.Name = "chkUseDiffViewerForBlame";
            this.chkUseDiffViewerForBlame.Size = new System.Drawing.Size(162, 19);
            this.chkUseDiffViewerForBlame.TabIndex = 5;
            this.chkUseDiffViewerForBlame.Text = "Show blame in diff viewer";
            this.chkUseDiffViewerForBlame.UseVisualStyleBackColor = true;
            // 
            // gbGeneral
            // 
            this.gbGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbGeneral.Controls.Add(this.cboTerminal);
            this.gbGeneral.Controls.Add(this.label2);
            this.gbGeneral.Controls.Add(this.chkUseBrowseForFileHistory);
            this.gbGeneral.Controls.Add(this.chkUseDiffViewerForBlame);
            this.gbGeneral.Location = new System.Drawing.Point(11, 11);
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.Size = new System.Drawing.Size(435, 104);
            this.gbGeneral.TabIndex = 1;
            this.gbGeneral.TabStop = false;
            this.gbGeneral.Text = "General";
            // 
            // gbTabs
            // 
            this.gbTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTabs.Controls.Add(this.chkShowGpgInformation);
            this.gbTabs.Controls.Add(this.chkChowConsoleTab);
            this.gbTabs.Location = new System.Drawing.Point(11, 121);
            this.gbTabs.Name = "gbTabs";
            this.gbTabs.Size = new System.Drawing.Size(435, 82);
            this.gbTabs.TabIndex = 5;
            this.gbTabs.TabStop = false;
            this.gbTabs.Text = "Tabs (restart required)";
            // 
            // FormBrowseRepoSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.gbTabs);
            this.Controls.Add(this.gbGeneral);
            this.Name = "FormBrowseRepoSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(457, 215);
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.gbTabs.ResumeLayout(false);
            this.gbTabs.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkShowGpgInformation;
        private System.Windows.Forms.CheckBox chkChowConsoleTab;
        private System.Windows.Forms.ComboBox cboTerminal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkUseBrowseForFileHistory;
        private System.Windows.Forms.CheckBox chkUseDiffViewerForBlame;
        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.GroupBox gbTabs;
    }
}
