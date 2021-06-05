
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
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Ok = new System.Windows.Forms.Button();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.flpnlLocalOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.rbDontChange = new System.Windows.Forms.RadioButton();
            this.rbMerge = new System.Windows.Forms.RadioButton();
            this.rbStash = new System.Windows.Forms.RadioButton();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.chkSetLocalChangesActionAsDefault = new System.Windows.Forms.CheckBox();
            this.tlpnlBranches = new System.Windows.Forms.TableLayoutPanel();
            this.LocalBranch = new System.Windows.Forms.RadioButton();
            this.Remotebranch = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.lbChanges = new System.Windows.Forms.Label();
            this.horLine = new System.Windows.Forms.Label();
            this.tlpnlRemoteOptions = new System.Windows.Forms.TableLayoutPanel();
            this.rbDontCreate = new System.Windows.Forms.RadioButton();
            this.txtCustomBranchName = new System.Windows.Forms.TextBox();
            this.rbResetBranch = new System.Windows.Forms.RadioButton();
            this.rbCreateBranchWithCustomName = new System.Windows.Forms.RadioButton();
            this.branchName = new System.Windows.Forms.Label();
            this.Errors = new System.Windows.Forms.ErrorProvider(this.components);
            this.MainPanel.SuspendLayout();
            this.tlpnlMain.SuspendLayout();
            this.localChangesGB.SuspendLayout();
            this.flpnlLocalOptions.SuspendLayout();
            this.tlpnlBranches.SuspendLayout();
            this.tlpnlRemoteOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Errors)).BeginInit();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.Ok);
            this.MainPanel.Controls.Add(this.tlpnlMain);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(14);
            this.MainPanel.Size = new System.Drawing.Size(626, 228);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(406, 151);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(74, 91);
            this.flowLayoutPanel1.TabIndex = 28;
            // 
            // Ok
            // 
            this.Ok.AutoSize = true;
            this.Ok.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(264, 5);
            this.Ok.MinimumSize = new System.Drawing.Size(75, 23);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "&Checkout";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlMain.Controls.Add(this.localChangesGB, 0, 3);
            this.tlpnlMain.Controls.Add(this.tlpnlBranches, 0, 0);
            this.tlpnlMain.Controls.Add(this.horLine, 0, 1);
            this.tlpnlMain.Controls.Add(this.tlpnlRemoteOptions, 0, 2);
            this.tlpnlMain.Location = new System.Drawing.Point(12, 12);
            this.tlpnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 4;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.Size = new System.Drawing.Size(600, 202);
            this.tlpnlMain.TabIndex = 0;
            this.tlpnlMain.TabStop = true;
            // 
            // localChangesGB
            // 
            this.localChangesGB.Controls.Add(this.flpnlLocalOptions);
            this.localChangesGB.Location = new System.Drawing.Point(0, 132);
            this.localChangesGB.Margin = new System.Windows.Forms.Padding(0);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Size = new System.Drawing.Size(441, 58);
            this.localChangesGB.TabIndex = 0;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            // 
            // flpnlLocalOptions
            // 
            this.flpnlLocalOptions.AutoSize = true;
            this.flpnlLocalOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpnlLocalOptions.Controls.Add(this.rbDontChange);
            this.flpnlLocalOptions.Controls.Add(this.rbMerge);
            this.flpnlLocalOptions.Controls.Add(this.rbStash);
            this.flpnlLocalOptions.Controls.Add(this.rbReset);
            this.flpnlLocalOptions.Controls.Add(this.chkSetLocalChangesActionAsDefault);
            this.flpnlLocalOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlLocalOptions.Location = new System.Drawing.Point(3, 16);
            this.flpnlLocalOptions.Name = "flpnlLocalOptions";
            this.flpnlLocalOptions.Padding = new System.Windows.Forms.Padding(9, 4, 9, 4);
            this.flpnlLocalOptions.Size = new System.Drawing.Size(435, 39);
            this.flpnlLocalOptions.TabIndex = 1;
            this.flpnlLocalOptions.WrapContents = false;
            // 
            // rbDontChange
            // 
            this.rbDontChange.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbDontChange.AutoSize = true;
            this.rbDontChange.Location = new System.Drawing.Point(11, 6);
            this.rbDontChange.Margin = new System.Windows.Forms.Padding(2);
            this.rbDontChange.Name = "rbDontChange";
            this.rbDontChange.Size = new System.Drawing.Size(89, 19);
            this.rbDontChange.TabIndex = 0;
            this.rbDontChange.Text = "Do&n\'t change";
            this.rbDontChange.UseVisualStyleBackColor = false;
            // 
            // rbMerge
            // 
            this.rbMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbMerge.AutoSize = true;
            this.rbMerge.Location = new System.Drawing.Point(104, 6);
            this.rbMerge.Margin = new System.Windows.Forms.Padding(2);
            this.rbMerge.Name = "rbMerge";
            this.rbMerge.Size = new System.Drawing.Size(55, 19);
            this.rbMerge.TabIndex = 1;
            this.rbMerge.Text = "&Merge";
            this.rbMerge.UseVisualStyleBackColor = false;
            // 
            // rbStash
            // 
            this.rbStash.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbStash.AutoSize = true;
            this.rbStash.Location = new System.Drawing.Point(163, 6);
            this.rbStash.Margin = new System.Windows.Forms.Padding(2);
            this.rbStash.Name = "rbStash";
            this.rbStash.Size = new System.Drawing.Size(52, 19);
            this.rbStash.TabIndex = 2;
            this.rbStash.Text = "S&tash";
            this.rbStash.UseVisualStyleBackColor = false;
            // 
            // rbReset
            // 
            this.rbReset.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(219, 6);
            this.rbReset.Margin = new System.Windows.Forms.Padding(2);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(53, 19);
            this.rbReset.TabIndex = 3;
            this.rbReset.Text = "&Reset";
            this.rbReset.UseVisualStyleBackColor = false;
            this.rbReset.CheckedChanged += new System.EventHandler(this.rbReset_CheckedChanged);
            // 
            // chkSetLocalChangesActionAsDefault
            // 
            this.chkSetLocalChangesActionAsDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSetLocalChangesActionAsDefault.AutoSize = true;
            this.chkSetLocalChangesActionAsDefault.Location = new System.Drawing.Point(277, 7);
            this.chkSetLocalChangesActionAsDefault.Name = "chkSetLocalChangesActionAsDefault";
            this.chkSetLocalChangesActionAsDefault.Size = new System.Drawing.Size(91, 17);
            this.chkSetLocalChangesActionAsDefault.TabIndex = 4;
            this.chkSetLocalChangesActionAsDefault.Text = "Set as &default";
            this.chkSetLocalChangesActionAsDefault.UseVisualStyleBackColor = false;
            // 
            // tlpnlBranches
            // 
            this.tlpnlBranches.ColumnCount = 3;
            this.tlpnlBranches.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlBranches.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlBranches.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlBranches.Controls.Add(this.LocalBranch, 0, 0);
            this.tlpnlBranches.Controls.Add(this.Remotebranch, 1, 0);
            this.tlpnlBranches.Controls.Add(this.label1, 0, 1);
            this.tlpnlBranches.Controls.Add(this.Branches, 1, 1);
            this.tlpnlBranches.Controls.Add(this.lbChanges, 2, 1);
            this.tlpnlBranches.Location = new System.Drawing.Point(0, 0);
            this.tlpnlBranches.Margin = new System.Windows.Forms.Padding(0);
            this.tlpnlBranches.Name = "tlpnlBranches";
            this.tlpnlBranches.RowCount = 2;
            this.tlpnlBranches.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBranches.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlpnlBranches.Size = new System.Drawing.Size(441, 54);
            this.tlpnlBranches.TabIndex = 0;
            // 
            // LocalBranch
            // 
            this.LocalBranch.AutoSize = true;
            this.LocalBranch.CausesValidation = false;
            this.LocalBranch.Checked = true;
            this.LocalBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LocalBranch.Location = new System.Drawing.Point(2, 2);
            this.LocalBranch.Margin = new System.Windows.Forms.Padding(2);
            this.LocalBranch.Name = "LocalBranch";
            this.LocalBranch.Size = new System.Drawing.Size(87, 17);
            this.LocalBranch.TabIndex = 0;
            this.LocalBranch.TabStop = true;
            this.LocalBranch.Text = "Local &branch";
            this.LocalBranch.UseVisualStyleBackColor = true;
            this.LocalBranch.CheckedChanged += new System.EventHandler(this.LocalBranchCheckedChanged);
            // 
            // Remotebranch
            // 
            this.Remotebranch.AutoSize = true;
            this.Remotebranch.CausesValidation = false;
            this.Remotebranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotebranch.Location = new System.Drawing.Point(93, 2);
            this.Remotebranch.Margin = new System.Windows.Forms.Padding(2);
            this.Remotebranch.Name = "Remotebranch";
            this.Remotebranch.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.Remotebranch.Size = new System.Drawing.Size(330, 17);
            this.Remotebranch.TabIndex = 1;
            this.Remotebranch.Text = "Remote &branch";
            this.Remotebranch.UseVisualStyleBackColor = true;
            this.Remotebranch.CheckedChanged += new System.EventHandler(this.RemoteBranchCheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 33);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Select branch";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Branches
            // 
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Branches.FormattingEnabled = true;
            this.Errors.SetIconAlignment(this.Branches, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.Branches.Location = new System.Drawing.Point(94, 24);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(328, 21);
            this.Branches.TabIndex = 3;
            this.Branches.SelectedIndexChanged += new System.EventHandler(this.Branches_SelectedIndexChanged);
            this.Branches.TextChanged += new System.EventHandler(this.Branches_TextChanged);
            this.Branches.Validating += new System.ComponentModel.CancelEventHandler(this.Branches_Validating);
            // 
            // lbChanges
            // 
            this.lbChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbChanges.AutoSize = true;
            this.lbChanges.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbChanges.Location = new System.Drawing.Point(428, 21);
            this.lbChanges.Name = "lbChanges";
            this.lbChanges.Size = new System.Drawing.Size(10, 33);
            this.lbChanges.TabIndex = 4;
            this.lbChanges.Text = "-";
            this.lbChanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // horLine
            // 
            this.horLine.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.horLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.horLine.Location = new System.Drawing.Point(3, 57);
            this.horLine.Margin = new System.Windows.Forms.Padding(3);
            this.horLine.Name = "horLine";
            this.horLine.Size = new System.Drawing.Size(684, 2);
            this.horLine.TabIndex = 1;
            // 
            // tlpnlRemoteOptions
            // 
            this.tlpnlRemoteOptions.AutoSize = true;
            this.tlpnlRemoteOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlRemoteOptions.ColumnCount = 2;
            this.tlpnlRemoteOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlRemoteOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlRemoteOptions.Controls.Add(this.rbDontCreate, 0, 4);
            this.tlpnlRemoteOptions.Controls.Add(this.txtCustomBranchName, 1, 2);
            this.tlpnlRemoteOptions.Controls.Add(this.rbResetBranch, 0, 0);
            this.tlpnlRemoteOptions.Controls.Add(this.rbCreateBranchWithCustomName, 0, 2);
            this.tlpnlRemoteOptions.Controls.Add(this.branchName, 1, 0);
            this.tlpnlRemoteOptions.Location = new System.Drawing.Point(0, 62);
            this.tlpnlRemoteOptions.Margin = new System.Windows.Forms.Padding(0);
            this.tlpnlRemoteOptions.Name = "tlpnlRemoteOptions";
            this.tlpnlRemoteOptions.RowCount = 5;
            this.tlpnlRemoteOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRemoteOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRemoteOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRemoteOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRemoteOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRemoteOptions.Size = new System.Drawing.Size(571, 70);
            this.tlpnlRemoteOptions.TabIndex = 2;
            // 
            // rbDontCreate
            // 
            this.rbDontCreate.AutoSize = true;
            this.rbDontCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbDontCreate.Location = new System.Drawing.Point(3, 50);
            this.rbDontCreate.Name = "rbDontCreate";
            this.rbDontCreate.Size = new System.Drawing.Size(208, 17);
            this.rbDontCreate.TabIndex = 4;
            this.rbDontCreate.Text = "Ch&eckout remote branch";
            this.rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // txtCustomBranchName
            // 
            this.txtCustomBranchName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCustomBranchName.Enabled = false;
            this.txtCustomBranchName.Location = new System.Drawing.Point(216, 25);
            this.txtCustomBranchName.Margin = new System.Windows.Forms.Padding(2);
            this.txtCustomBranchName.Name = "txtCustomBranchName";
            this.txtCustomBranchName.Size = new System.Drawing.Size(353, 20);
            this.txtCustomBranchName.TabIndex = 3;
            this.txtCustomBranchName.Leave += new System.EventHandler(this.txtCustomBranchName_Leave);
            // 
            // rbResetBranch
            // 
            this.rbResetBranch.AutoSize = true;
            this.rbResetBranch.Checked = true;
            this.rbResetBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbResetBranch.Location = new System.Drawing.Point(3, 3);
            this.rbResetBranch.Name = "rbResetBranch";
            this.rbResetBranch.Size = new System.Drawing.Size(208, 17);
            this.rbResetBranch.TabIndex = 0;
            this.rbResetBranch.TabStop = true;
            this.rbResetBranch.Text = "R&eset local branch with the name:";
            this.rbResetBranch.UseVisualStyleBackColor = true;
            // 
            // rbCreateBranchWithCustomName
            // 
            this.rbCreateBranchWithCustomName.AutoSize = true;
            this.rbCreateBranchWithCustomName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbCreateBranchWithCustomName.Location = new System.Drawing.Point(3, 26);
            this.rbCreateBranchWithCustomName.Margin = new System.Windows.Forms.Padding(3, 3, 3, 4);
            this.rbCreateBranchWithCustomName.Name = "rbCreateBranchWithCustomName";
            this.rbCreateBranchWithCustomName.Size = new System.Drawing.Size(208, 17);
            this.rbCreateBranchWithCustomName.TabIndex = 2;
            this.rbCreateBranchWithCustomName.Text = "Cr&eate local branch with custom name:";
            this.rbCreateBranchWithCustomName.UseVisualStyleBackColor = true;
            this.rbCreateBranchWithCustomName.CheckedChanged += new System.EventHandler(this.rbCreateBranchWithCustomName_CheckedChanged);
            // 
            // branchName
            // 
            this.branchName.AutoEllipsis = true;
            this.branchName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchName.Location = new System.Drawing.Point(217, 0);
            this.branchName.Name = "branchName";
            this.branchName.Size = new System.Drawing.Size(351, 23);
            this.branchName.TabIndex = 24;
            this.branchName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Errors
            // 
            this.Errors.ContainerControl = this;
            // 
            // FormCheckoutBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(626, 260);
            this.HelpButton = true;
            this.ManualSectionAnchorName = "checkout-branch";
            this.ManualSectionSubfolder = "branches";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCheckoutBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout branch";
            this.Activated += new System.EventHandler(this.FormCheckoutBranch_Activated);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.localChangesGB.ResumeLayout(false);
            this.localChangesGB.PerformLayout();
            this.flpnlLocalOptions.ResumeLayout(false);
            this.flpnlLocalOptions.PerformLayout();
            this.tlpnlBranches.ResumeLayout(false);
            this.tlpnlBranches.PerformLayout();
            this.tlpnlRemoteOptions.ResumeLayout(false);
            this.tlpnlRemoteOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Errors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label horLine;
        private System.Windows.Forms.TableLayoutPanel tlpnlBranches;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
        private System.Windows.Forms.TableLayoutPanel tlpnlRemoteOptions;
        private System.Windows.Forms.RadioButton rbDontCreate;
        private System.Windows.Forms.TextBox txtCustomBranchName;
        private System.Windows.Forms.RadioButton rbResetBranch;
        private System.Windows.Forms.RadioButton rbCreateBranchWithCustomName;
        private System.Windows.Forms.Label branchName;
        private System.Windows.Forms.GroupBox localChangesGB;
        private System.Windows.Forms.RadioButton rbReset;
        private System.Windows.Forms.RadioButton rbStash;
        private System.Windows.Forms.RadioButton rbMerge;
        private System.Windows.Forms.RadioButton rbDontChange;
        private System.Windows.Forms.RadioButton LocalBranch;
        private System.Windows.Forms.RadioButton Remotebranch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbChanges;
        private System.Windows.Forms.CheckBox chkSetLocalChangesActionAsDefault;
        private FlowLayoutPanel flpnlLocalOptions;
        private ErrorProvider Errors;
    }
}
