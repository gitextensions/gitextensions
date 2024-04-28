
using System.Windows.Forms;
namespace GitUI.CommandsDialogs
{
    partial class FormCheckoutBranch
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
            components = new System.ComponentModel.Container();
            flowLayoutPanel1 = new FlowLayoutPanel();
            Ok = new Button();
            tlpnlMain = new TableLayoutPanel();
            localChangesGB = new GroupBox();
            flpnlLocalOptions = new FlowLayoutPanel();
            rbDontChange = new RadioButton();
            rbMerge = new RadioButton();
            rbStash = new RadioButton();
            rbReset = new RadioButton();
            chkSetLocalChangesActionAsDefault = new CheckBox();
            tlpnlBranches = new TableLayoutPanel();
            LocalBranch = new RadioButton();
            Remotebranch = new RadioButton();
            label1 = new Label();
            Branches = new ComboBox();
            lbChanges = new Label();
            horLine = new Label();
            tlpnlRemoteOptions = new TableLayoutPanel();
            rbDontCreate = new RadioButton();
            txtCustomBranchName = new TextBox();
            rbResetBranch = new RadioButton();
            rbCreateBranchWithCustomName = new RadioButton();
            branchName = new Label();
            Errors = new ErrorProvider(components);
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlpnlMain.SuspendLayout();
            localChangesGB.SuspendLayout();
            flpnlLocalOptions.SuspendLayout();
            tlpnlBranches.SuspendLayout();
            tlpnlRemoteOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(Errors)).BeginInit();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tlpnlMain);
            MainPanel.Padding = new Padding(14);
            MainPanel.Size = new Size(626, 219);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Ok);
            ControlsPanel.Location = new Point(0, 219);
            ControlsPanel.Size = new Size(626, 41);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.BottomUp;
            flowLayoutPanel1.Location = new Point(406, 151);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(74, 91);
            flowLayoutPanel1.TabIndex = 28;
            // 
            // Ok
            // 
            Ok.AutoSize = true;
            Ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(538, 8);
            Ok.MinimumSize = new Size(75, 23);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 25);
            Ok.TabIndex = 1;
            Ok.Text = "&Checkout";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // tlpnlMain
            // 
            tlpnlMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(localChangesGB, 0, 3);
            tlpnlMain.Controls.Add(tlpnlBranches, 0, 0);
            tlpnlMain.Controls.Add(horLine, 0, 1);
            tlpnlMain.Controls.Add(tlpnlRemoteOptions, 0, 2);
            tlpnlMain.Location = new Point(12, 12);
            tlpnlMain.Margin = new Padding(0);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.Size = new Size(600, 202);
            tlpnlMain.TabIndex = 0;
            tlpnlMain.TabStop = true;
            // 
            // localChangesGB
            // 
            localChangesGB.Controls.Add(flpnlLocalOptions);
            localChangesGB.Location = new Point(0, 139);
            localChangesGB.Margin = new Padding(0);
            localChangesGB.Name = "localChangesGB";
            localChangesGB.Size = new Size(441, 58);
            localChangesGB.TabIndex = 0;
            localChangesGB.TabStop = false;
            localChangesGB.Text = "Local changes";
            // 
            // flpnlLocalOptions
            // 
            flpnlLocalOptions.AutoSize = true;
            flpnlLocalOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpnlLocalOptions.Controls.Add(rbDontChange);
            flpnlLocalOptions.Controls.Add(rbMerge);
            flpnlLocalOptions.Controls.Add(rbStash);
            flpnlLocalOptions.Controls.Add(rbReset);
            flpnlLocalOptions.Controls.Add(chkSetLocalChangesActionAsDefault);
            flpnlLocalOptions.Dock = DockStyle.Fill;
            flpnlLocalOptions.Location = new Point(3, 19);
            flpnlLocalOptions.Name = "flpnlLocalOptions";
            flpnlLocalOptions.Padding = new Padding(9, 4, 9, 4);
            flpnlLocalOptions.Size = new Size(435, 36);
            flpnlLocalOptions.TabIndex = 1;
            flpnlLocalOptions.WrapContents = false;
            // 
            // rbDontChange
            // 
            rbDontChange.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rbDontChange.AutoSize = true;
            rbDontChange.Location = new Point(11, 6);
            rbDontChange.Margin = new Padding(2);
            rbDontChange.Name = "rbDontChange";
            rbDontChange.Size = new Size(96, 21);
            rbDontChange.TabIndex = 0;
            rbDontChange.Text = "Do&n\'t change";
            rbDontChange.UseVisualStyleBackColor = false;
            // 
            // rbMerge
            // 
            rbMerge.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rbMerge.AutoSize = true;
            rbMerge.Location = new Point(111, 6);
            rbMerge.Margin = new Padding(2);
            rbMerge.Name = "rbMerge";
            rbMerge.Size = new Size(59, 21);
            rbMerge.TabIndex = 1;
            rbMerge.Text = "&Merge";
            rbMerge.UseVisualStyleBackColor = false;
            // 
            // rbStash
            // 
            rbStash.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rbStash.AutoSize = true;
            rbStash.Location = new Point(174, 6);
            rbStash.Margin = new Padding(2);
            rbStash.Name = "rbStash";
            rbStash.Size = new Size(53, 21);
            rbStash.TabIndex = 2;
            rbStash.Text = "S&tash";
            rbStash.UseVisualStyleBackColor = false;
            // 
            // rbReset
            // 
            rbReset.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rbReset.AutoSize = true;
            rbReset.Location = new Point(231, 6);
            rbReset.Margin = new Padding(2);
            rbReset.Name = "rbReset";
            rbReset.Size = new Size(53, 21);
            rbReset.TabIndex = 3;
            rbReset.Text = "&Reset";
            rbReset.UseVisualStyleBackColor = false;
            rbReset.CheckedChanged += rbReset_CheckedChanged;
            // 
            // chkSetLocalChangesActionAsDefault
            // 
            chkSetLocalChangesActionAsDefault.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chkSetLocalChangesActionAsDefault.AutoSize = true;
            chkSetLocalChangesActionAsDefault.Location = new Point(289, 7);
            chkSetLocalChangesActionAsDefault.Name = "chkSetLocalChangesActionAsDefault";
            chkSetLocalChangesActionAsDefault.Size = new Size(96, 19);
            chkSetLocalChangesActionAsDefault.TabIndex = 4;
            chkSetLocalChangesActionAsDefault.Text = "Set as &default";
            chkSetLocalChangesActionAsDefault.UseVisualStyleBackColor = false;
            // 
            // tlpnlBranches
            // 
            tlpnlBranches.ColumnCount = 3;
            tlpnlBranches.ColumnStyles.Add(new ColumnStyle());
            tlpnlBranches.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlBranches.ColumnStyles.Add(new ColumnStyle());
            tlpnlBranches.Controls.Add(LocalBranch, 0, 0);
            tlpnlBranches.Controls.Add(Remotebranch, 1, 0);
            tlpnlBranches.Controls.Add(label1, 0, 1);
            tlpnlBranches.Controls.Add(Branches, 1, 1);
            tlpnlBranches.Controls.Add(lbChanges, 2, 1);
            tlpnlBranches.Location = new Point(0, 0);
            tlpnlBranches.Margin = new Padding(0);
            tlpnlBranches.Name = "tlpnlBranches";
            tlpnlBranches.RowCount = 2;
            tlpnlBranches.RowStyles.Add(new RowStyle());
            tlpnlBranches.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            tlpnlBranches.Size = new Size(441, 54);
            tlpnlBranches.TabIndex = 0;
            // 
            // LocalBranch
            // 
            LocalBranch.AutoSize = true;
            LocalBranch.CausesValidation = false;
            LocalBranch.Checked = true;
            LocalBranch.Dock = DockStyle.Fill;
            LocalBranch.Location = new Point(2, 2);
            LocalBranch.Margin = new Padding(2);
            LocalBranch.Name = "LocalBranch";
            LocalBranch.Size = new Size(93, 19);
            LocalBranch.TabIndex = 0;
            LocalBranch.TabStop = true;
            LocalBranch.Text = "Local &branch";
            LocalBranch.UseVisualStyleBackColor = true;
            LocalBranch.CheckedChanged += LocalBranchCheckedChanged;
            // 
            // Remotebranch
            // 
            Remotebranch.AutoSize = true;
            Remotebranch.CausesValidation = false;
            Remotebranch.Dock = DockStyle.Fill;
            Remotebranch.Location = new Point(99, 2);
            Remotebranch.Margin = new Padding(2);
            Remotebranch.Name = "Remotebranch";
            Remotebranch.Padding = new Padding(6, 0, 0, 0);
            Remotebranch.Size = new Size(322, 19);
            Remotebranch.TabIndex = 1;
            Remotebranch.Text = "Remote &branch";
            Remotebranch.UseVisualStyleBackColor = true;
            Remotebranch.CheckedChanged += RemoteBranchCheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 23);
            label1.Name = "label1";
            label1.Size = new Size(91, 31);
            label1.TabIndex = 2;
            label1.Text = "&Select branch";
            label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Branches
            // 
            Branches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Branches.AutoCompleteSource = AutoCompleteSource.ListItems;
            Branches.Dock = DockStyle.Fill;
            Branches.FormattingEnabled = true;
            Errors.SetIconAlignment(Branches, ErrorIconAlignment.MiddleLeft);
            Branches.Location = new Point(100, 26);
            Branches.Name = "Branches";
            Branches.Size = new Size(320, 23);
            Branches.TabIndex = 3;
            Branches.SelectedIndexChanged += Branches_SelectedIndexChanged;
            Branches.TextChanged += Branches_TextChanged;
            Branches.Validating += Branches_Validating;
            // 
            // lbChanges
            // 
            lbChanges.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbChanges.AutoSize = true;
            lbChanges.ForeColor = SystemColors.GrayText;
            lbChanges.Location = new Point(426, 23);
            lbChanges.Name = "lbChanges";
            lbChanges.Size = new Size(12, 31);
            lbChanges.TabIndex = 4;
            lbChanges.Text = "-";
            lbChanges.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // horLine
            // 
            horLine.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            horLine.BorderStyle = BorderStyle.Fixed3D;
            horLine.Location = new Point(3, 57);
            horLine.Margin = new Padding(3);
            horLine.Name = "horLine";
            horLine.Size = new Size(684, 2);
            horLine.TabIndex = 1;
            // 
            // tlpnlRemoteOptions
            // 
            tlpnlRemoteOptions.AutoSize = true;
            tlpnlRemoteOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlRemoteOptions.ColumnCount = 2;
            tlpnlRemoteOptions.ColumnStyles.Add(new ColumnStyle());
            tlpnlRemoteOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlRemoteOptions.Controls.Add(rbDontCreate, 0, 4);
            tlpnlRemoteOptions.Controls.Add(txtCustomBranchName, 1, 2);
            tlpnlRemoteOptions.Controls.Add(rbResetBranch, 0, 0);
            tlpnlRemoteOptions.Controls.Add(rbCreateBranchWithCustomName, 0, 2);
            tlpnlRemoteOptions.Controls.Add(branchName, 1, 0);
            tlpnlRemoteOptions.Location = new Point(0, 62);
            tlpnlRemoteOptions.Margin = new Padding(0);
            tlpnlRemoteOptions.Name = "tlpnlRemoteOptions";
            tlpnlRemoteOptions.RowCount = 5;
            tlpnlRemoteOptions.RowStyles.Add(new RowStyle());
            tlpnlRemoteOptions.RowStyles.Add(new RowStyle());
            tlpnlRemoteOptions.RowStyles.Add(new RowStyle());
            tlpnlRemoteOptions.RowStyles.Add(new RowStyle());
            tlpnlRemoteOptions.RowStyles.Add(new RowStyle());
            tlpnlRemoteOptions.Size = new Size(595, 77);
            tlpnlRemoteOptions.TabIndex = 2;
            // 
            // rbDontCreate
            // 
            rbDontCreate.AutoSize = true;
            rbDontCreate.Dock = DockStyle.Fill;
            rbDontCreate.Location = new Point(3, 55);
            rbDontCreate.Name = "rbDontCreate";
            rbDontCreate.Size = new Size(232, 19);
            rbDontCreate.TabIndex = 4;
            rbDontCreate.Text = "Ch&eckout the commit (in detached head)";
            rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // txtCustomBranchName
            // 
            txtCustomBranchName.Dock = DockStyle.Fill;
            txtCustomBranchName.Enabled = false;
            txtCustomBranchName.Location = new Point(240, 27);
            txtCustomBranchName.Margin = new Padding(2);
            txtCustomBranchName.Name = "txtCustomBranchName";
            txtCustomBranchName.Size = new Size(353, 23);
            txtCustomBranchName.TabIndex = 3;
            txtCustomBranchName.Leave += txtCustomBranchName_Leave;
            // 
            // rbResetBranch
            // 
            rbResetBranch.AutoSize = true;
            rbResetBranch.Checked = true;
            rbResetBranch.Dock = DockStyle.Fill;
            rbResetBranch.Location = new Point(3, 3);
            rbResetBranch.Name = "rbResetBranch";
            rbResetBranch.Size = new Size(232, 19);
            rbResetBranch.TabIndex = 0;
            rbResetBranch.TabStop = true;
            rbResetBranch.Text = "R&eset local branch with the name:";
            rbResetBranch.UseVisualStyleBackColor = true;
            // 
            // rbCreateBranchWithCustomName
            // 
            rbCreateBranchWithCustomName.AutoSize = true;
            rbCreateBranchWithCustomName.Dock = DockStyle.Fill;
            rbCreateBranchWithCustomName.Location = new Point(3, 28);
            rbCreateBranchWithCustomName.Margin = new Padding(3, 3, 3, 4);
            rbCreateBranchWithCustomName.Name = "rbCreateBranchWithCustomName";
            rbCreateBranchWithCustomName.Size = new Size(232, 20);
            rbCreateBranchWithCustomName.TabIndex = 2;
            rbCreateBranchWithCustomName.Text = "Cr&eate local branch with custom name:";
            rbCreateBranchWithCustomName.UseVisualStyleBackColor = true;
            rbCreateBranchWithCustomName.CheckedChanged += rbCreateBranchWithCustomName_CheckedChanged;
            // 
            // branchName
            // 
            branchName.AutoEllipsis = true;
            branchName.Dock = DockStyle.Fill;
            branchName.Location = new Point(241, 0);
            branchName.Name = "branchName";
            branchName.Size = new Size(351, 25);
            branchName.TabIndex = 24;
            branchName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Errors
            // 
            Errors.ContainerControl = this;
            // 
            // FormCheckoutBranch
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(626, 260);
            HelpButton = true;
            ManualSectionAnchorName = "checkout-branch";
            ManualSectionSubfolder = "branches";
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCheckoutBranch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Checkout branch";
            Activated += FormCheckoutBranch_Activated;
            MainPanel.ResumeLayout(false);
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            localChangesGB.ResumeLayout(false);
            localChangesGB.PerformLayout();
            flpnlLocalOptions.ResumeLayout(false);
            flpnlLocalOptions.PerformLayout();
            tlpnlBranches.ResumeLayout(false);
            tlpnlBranches.PerformLayout();
            tlpnlRemoteOptions.ResumeLayout(false);
            tlpnlRemoteOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(Errors)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ComboBox Branches;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button Ok;
        private Label horLine;
        private TableLayoutPanel tlpnlBranches;
        private TableLayoutPanel tlpnlMain;
        private TableLayoutPanel tlpnlRemoteOptions;
        private RadioButton rbDontCreate;
        private TextBox txtCustomBranchName;
        private RadioButton rbResetBranch;
        private RadioButton rbCreateBranchWithCustomName;
        private Label branchName;
        private GroupBox localChangesGB;
        private RadioButton rbReset;
        private RadioButton rbStash;
        private RadioButton rbMerge;
        private RadioButton rbDontChange;
        private RadioButton LocalBranch;
        private RadioButton Remotebranch;
        private Label label1;
        private Label lbChanges;
        private CheckBox chkSetLocalChangesActionAsDefault;
        private FlowLayoutPanel flpnlLocalOptions;
        private ErrorProvider Errors;
    }
}
