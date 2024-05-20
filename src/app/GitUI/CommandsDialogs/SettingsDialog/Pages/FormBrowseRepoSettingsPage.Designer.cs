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
            chkShowGpgInformation = new CheckBox();
            chkChowConsoleTab = new CheckBox();
            cboTerminal = new ComboBox();
            label2 = new Label();
            chkUseBrowseForFileHistory = new CheckBox();
            chkUseDiffViewerForBlame = new CheckBox();
            gbGeneral = new GroupBox();
            gbTabs = new GroupBox();
            gbGeneral.SuspendLayout();
            gbTabs.SuspendLayout();
            SuspendLayout();
            // 
            // chkShowGpgInformation
            // 
            chkShowGpgInformation.AutoSize = true;
            chkShowGpgInformation.Location = new Point(9, 48);
            chkShowGpgInformation.Name = "chkShowGpgInformation";
            chkShowGpgInformation.Size = new Size(147, 19);
            chkShowGpgInformation.TabIndex = 7;
            chkShowGpgInformation.Text = "Show GPG information";
            chkShowGpgInformation.UseVisualStyleBackColor = true;
            // 
            // chkChowConsoleTab
            // 
            chkChowConsoleTab.AutoSize = true;
            chkChowConsoleTab.Location = new Point(9, 25);
            chkChowConsoleTab.Name = "chkChowConsoleTab";
            chkChowConsoleTab.Size = new Size(141, 19);
            chkChowConsoleTab.TabIndex = 6;
            chkChowConsoleTab.Text = "Show the Console tab";
            chkChowConsoleTab.UseVisualStyleBackColor = true;
            // 
            // cboTerminal
            // 
            cboTerminal.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTerminal.FormattingEnabled = true;
            cboTerminal.Location = new Point(146, 22);
            cboTerminal.Margin = new Padding(3, 2, 3, 2);
            cboTerminal.Name = "cboTerminal";
            cboTerminal.Size = new Size(262, 23);
            cboTerminal.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 25);
            label2.Name = "label2";
            label2.Size = new Size(72, 15);
            label2.TabIndex = 2;
            label2.Text = "Default shell";
            // 
            // chkUseBrowseForFileHistory
            // 
            chkUseBrowseForFileHistory.AutoSize = true;
            chkUseBrowseForFileHistory.Location = new Point(9, 48);
            chkUseBrowseForFileHistory.Name = "chkUseBrowseForFileHistory";
            chkUseBrowseForFileHistory.Size = new Size(165, 19);
            chkUseBrowseForFileHistory.TabIndex = 4;
            chkUseBrowseForFileHistory.Text = "Show file history in the main window";
            chkUseBrowseForFileHistory.UseVisualStyleBackColor = true;
            // 
            // chkUseDiffViewerForBlame
            // 
            chkUseDiffViewerForBlame.AutoSize = true;
            chkUseDiffViewerForBlame.Location = new Point(9, 71);
            chkUseDiffViewerForBlame.Name = "chkUseDiffViewerForBlame";
            chkUseDiffViewerForBlame.Size = new Size(162, 19);
            chkUseDiffViewerForBlame.TabIndex = 5;
            chkUseDiffViewerForBlame.Text = "Show blame in diff viewer";
            chkUseDiffViewerForBlame.UseVisualStyleBackColor = true;
            // 
            // gbGeneral
            // 
            gbGeneral.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbGeneral.Controls.Add(cboTerminal);
            gbGeneral.Controls.Add(label2);
            gbGeneral.Controls.Add(chkUseBrowseForFileHistory);
            gbGeneral.Controls.Add(chkUseDiffViewerForBlame);
            gbGeneral.Location = new Point(11, 11);
            gbGeneral.Name = "gbGeneral";
            gbGeneral.Size = new Size(435, 104);
            gbGeneral.TabIndex = 1;
            gbGeneral.TabStop = false;
            gbGeneral.Text = "General";
            // 
            // gbTabs
            // 
            gbTabs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbTabs.Controls.Add(chkShowGpgInformation);
            gbTabs.Controls.Add(chkChowConsoleTab);
            gbTabs.Location = new Point(11, 121);
            gbTabs.Name = "gbTabs";
            gbTabs.Size = new Size(435, 82);
            gbTabs.TabIndex = 5;
            gbTabs.TabStop = false;
            gbTabs.Text = "Tabs (restart required)";
            // 
            // FormBrowseRepoSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(gbTabs);
            Controls.Add(gbGeneral);
            Name = "FormBrowseRepoSettingsPage";
            Padding = new Padding(8);
            Size = new Size(457, 215);
            Text = "Browse repository window";
            gbGeneral.ResumeLayout(false);
            gbGeneral.PerformLayout();
            gbTabs.ResumeLayout(false);
            gbTabs.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private CheckBox chkShowGpgInformation;
        private CheckBox chkChowConsoleTab;
        private ComboBox cboTerminal;
        private Label label2;
        private CheckBox chkUseBrowseForFileHistory;
        private CheckBox chkUseDiffViewerForBlame;
        private GroupBox gbGeneral;
        private GroupBox gbTabs;
    }
}
