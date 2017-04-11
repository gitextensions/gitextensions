
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
            this.Branches = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Ok = new System.Windows.Forms.Button();
            this.horLine = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Remotebranch = new System.Windows.Forms.RadioButton();
            this.LocalBranch = new System.Windows.Forms.RadioButton();
            this.setBranchPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lbChanges = new System.Windows.Forms.Label();
            this.localChangesPanel = new System.Windows.Forms.TableLayoutPanel();
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.rbStash = new System.Windows.Forms.RadioButton();
            this.rbMerge = new System.Windows.Forms.RadioButton();
            this.rbDontChange = new System.Windows.Forms.RadioButton();
            this.chkSetLocalChangesActionAsDefault = new System.Windows.Forms.CheckBox();
            this.remoteOptionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rbDontCreate = new System.Windows.Forms.RadioButton();
            this.txtCustomBranchName = new System.Windows.Forms.TextBox();
            this.rbResetBranch = new System.Windows.Forms.RadioButton();
            this.rbCreateBranchWithCustomName = new System.Windows.Forms.RadioButton();
            this.branchName = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.setBranchPanel.SuspendLayout();
            this.localChangesPanel.SuspendLayout();
            this.localChangesGB.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.remoteOptionsPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Branches
            // 
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(113, 37);
            this.Branches.Margin = new System.Windows.Forms.Padding(2, 12, 8, 8);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(296, 24);
            this.Branches.TabIndex = 3;
            this.Branches.SelectedIndexChanged += new System.EventHandler(this.Branches_SelectedIndexChanged);
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
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.AutoSize = true;
            this.Ok.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(460, 40);
            this.Ok.Margin = new System.Windows.Forms.Padding(0, 12, 8, 8);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(77, 27);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Checkout";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // horLine
            // 
            this.horLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel1.SetColumnSpan(this.horLine, 3);
            this.horLine.Location = new System.Drawing.Point(13, 91);
            this.horLine.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            this.horLine.Name = "horLine";
            this.horLine.Size = new System.Drawing.Size(584, 2);
            this.horLine.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 44);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 12, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select branch";
            // 
            // Remotebranch
            // 
            this.Remotebranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Remotebranch.AutoSize = true;
            this.Remotebranch.Location = new System.Drawing.Point(113, 2);
            this.Remotebranch.Margin = new System.Windows.Forms.Padding(2);
            this.Remotebranch.Name = "Remotebranch";
            this.Remotebranch.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.Remotebranch.Size = new System.Drawing.Size(132, 21);
            this.Remotebranch.TabIndex = 1;
            this.Remotebranch.Text = "Remote branch";
            this.Remotebranch.UseVisualStyleBackColor = true;
            this.Remotebranch.CheckedChanged += new System.EventHandler(this.RemoteBranchCheckedChanged);
            // 
            // LocalBranch
            // 
            this.LocalBranch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LocalBranch.AutoSize = true;
            this.LocalBranch.Checked = true;
            this.LocalBranch.Location = new System.Drawing.Point(2, 2);
            this.LocalBranch.Margin = new System.Windows.Forms.Padding(2);
            this.LocalBranch.Name = "LocalBranch";
            this.LocalBranch.Size = new System.Drawing.Size(107, 21);
            this.LocalBranch.TabIndex = 0;
            this.LocalBranch.TabStop = true;
            this.LocalBranch.Text = "Local branch";
            this.LocalBranch.UseVisualStyleBackColor = true;
            this.LocalBranch.CheckedChanged += new System.EventHandler(this.LocalBranchCheckedChanged);
            // 
            // setBranchPanel
            // 
            this.setBranchPanel.AutoSize = true;
            this.setBranchPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.setBranchPanel.ColumnCount = 3;
            this.tableLayoutPanel1.SetColumnSpan(this.setBranchPanel, 3);
            this.setBranchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.setBranchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.setBranchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.setBranchPanel.Controls.Add(this.LocalBranch, 0, 0);
            this.setBranchPanel.Controls.Add(this.Remotebranch, 1, 0);
            this.setBranchPanel.Controls.Add(this.label1, 0, 1);
            this.setBranchPanel.Controls.Add(this.Branches, 1, 1);
            this.setBranchPanel.Controls.Add(this.lbChanges, 2, 1);
            this.setBranchPanel.Location = new System.Drawing.Point(9, 11);
            this.setBranchPanel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.setBranchPanel.Name = "setBranchPanel";
            this.setBranchPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.setBranchPanel.RowCount = 2;
            this.setBranchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.setBranchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.setBranchPanel.Size = new System.Drawing.Size(440, 74);
            this.setBranchPanel.TabIndex = 0;
            // 
            // lbChanges
            // 
            this.lbChanges.AutoSize = true;
            this.lbChanges.Dock = System.Windows.Forms.DockStyle.Right;
            this.lbChanges.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbChanges.Location = new System.Drawing.Point(419, 43);
            this.lbChanges.Margin = new System.Windows.Forms.Padding(2, 18, 8, 2);
            this.lbChanges.Name = "lbChanges";
            this.lbChanges.Size = new System.Drawing.Size(13, 24);
            this.lbChanges.TabIndex = 4;
            this.lbChanges.Text = "-";
            // 
            // localChangesPanel
            // 
            this.localChangesPanel.AutoSize = true;
            this.localChangesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.localChangesPanel.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.localChangesPanel, 3);
            this.localChangesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.localChangesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.localChangesPanel.Controls.Add(this.localChangesGB, 0, 0);
            this.localChangesPanel.Controls.Add(this.Ok, 1, 0);
            this.localChangesPanel.Location = new System.Drawing.Point(9, 185);
            this.localChangesPanel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.localChangesPanel.Name = "localChangesPanel";
            this.localChangesPanel.RowCount = 1;
            this.localChangesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.localChangesPanel.Size = new System.Drawing.Size(545, 75);
            this.localChangesPanel.TabIndex = 3;
            // 
            // localChangesGB
            // 
            this.localChangesGB.AutoSize = true;
            this.localChangesGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.localChangesGB.Controls.Add(this.tableLayoutPanel2);
            this.localChangesGB.Location = new System.Drawing.Point(2, 12);
            this.localChangesGB.Margin = new System.Windows.Forms.Padding(2, 12, 2, 2);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.localChangesGB.Size = new System.Drawing.Size(456, 61);
            this.localChangesGB.TabIndex = 0;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.rbReset, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.rbStash, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.rbMerge, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.rbDontChange, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.chkSetLocalChangesActionAsDefault, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(8, 24);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(440, 29);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // rbReset
            // 
            this.rbReset.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(255, 4);
            this.rbReset.Margin = new System.Windows.Forms.Padding(2);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(63, 21);
            this.rbReset.TabIndex = 3;
            this.rbReset.Text = "Reset";
            this.rbReset.UseVisualStyleBackColor = true;
            this.rbReset.CheckedChanged += new System.EventHandler(this.rbReset_CheckedChanged);
            // 
            // rbStash
            // 
            this.rbStash.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbStash.AutoSize = true;
            this.rbStash.Location = new System.Drawing.Point(188, 4);
            this.rbStash.Margin = new System.Windows.Forms.Padding(2);
            this.rbStash.Name = "rbStash";
            this.rbStash.Size = new System.Drawing.Size(63, 21);
            this.rbStash.TabIndex = 2;
            this.rbStash.Text = "Stash";
            this.rbStash.UseVisualStyleBackColor = true;
            // 
            // rbMerge
            // 
            this.rbMerge.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbMerge.AutoSize = true;
            this.rbMerge.Location = new System.Drawing.Point(118, 4);
            this.rbMerge.Margin = new System.Windows.Forms.Padding(2);
            this.rbMerge.Name = "rbMerge";
            this.rbMerge.Size = new System.Drawing.Size(66, 21);
            this.rbMerge.TabIndex = 1;
            this.rbMerge.Text = "Merge";
            this.rbMerge.UseVisualStyleBackColor = true;
            // 
            // rbDontChange
            // 
            this.rbDontChange.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbDontChange.AutoSize = true;
            this.rbDontChange.Location = new System.Drawing.Point(2, 4);
            this.rbDontChange.Margin = new System.Windows.Forms.Padding(2);
            this.rbDontChange.Name = "rbDontChange";
            this.rbDontChange.Size = new System.Drawing.Size(112, 21);
            this.rbDontChange.TabIndex = 0;
            this.rbDontChange.Text = "Don\'t change";
            this.rbDontChange.UseVisualStyleBackColor = true;
            // 
            // chkSetLocalChangesActionAsDefault
            // 
            this.chkSetLocalChangesActionAsDefault.AutoSize = true;
            this.chkSetLocalChangesActionAsDefault.Location = new System.Drawing.Point(324, 4);
            this.chkSetLocalChangesActionAsDefault.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkSetLocalChangesActionAsDefault.Name = "chkSetLocalChangesActionAsDefault";
            this.chkSetLocalChangesActionAsDefault.Size = new System.Drawing.Size(112, 21);
            this.chkSetLocalChangesActionAsDefault.TabIndex = 4;
            this.chkSetLocalChangesActionAsDefault.Text = "Set as default";
            this.chkSetLocalChangesActionAsDefault.UseVisualStyleBackColor = true;
            // 
            // remoteOptionsPanel
            // 
            this.remoteOptionsPanel.AutoSize = true;
            this.remoteOptionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.remoteOptionsPanel.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.remoteOptionsPanel, 3);
            this.remoteOptionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.remoteOptionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.remoteOptionsPanel.Controls.Add(this.rbDontCreate, 0, 4);
            this.remoteOptionsPanel.Controls.Add(this.txtCustomBranchName, 1, 2);
            this.remoteOptionsPanel.Controls.Add(this.rbResetBranch, 0, 0);
            this.remoteOptionsPanel.Controls.Add(this.rbCreateBranchWithCustomName, 0, 2);
            this.remoteOptionsPanel.Controls.Add(this.branchName, 1, 0);
            this.remoteOptionsPanel.Location = new System.Drawing.Point(9, 95);
            this.remoteOptionsPanel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.remoteOptionsPanel.Name = "remoteOptionsPanel";
            this.remoteOptionsPanel.RowCount = 5;
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.Size = new System.Drawing.Size(583, 88);
            this.remoteOptionsPanel.TabIndex = 2;
            this.remoteOptionsPanel.Visible = false;
            // 
            // rbDontCreate
            // 
            this.rbDontCreate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rbDontCreate.AutoSize = true;
            this.rbDontCreate.Location = new System.Drawing.Point(4, 63);
            this.rbDontCreate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbDontCreate.Name = "rbDontCreate";
            this.rbDontCreate.Size = new System.Drawing.Size(183, 21);
            this.rbDontCreate.TabIndex = 4;
            this.rbDontCreate.Text = "Checkout remote branch";
            this.rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // txtCustomBranchName
            // 
            this.txtCustomBranchName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCustomBranchName.Enabled = false;
            this.txtCustomBranchName.Location = new System.Drawing.Point(278, 31);
            this.txtCustomBranchName.Margin = new System.Windows.Forms.Padding(2);
            this.txtCustomBranchName.Name = "txtCustomBranchName";
            this.txtCustomBranchName.Size = new System.Drawing.Size(303, 23);
            this.txtCustomBranchName.TabIndex = 3;
            this.txtCustomBranchName.Leave += new System.EventHandler(this.txtCustomBranchName_Leave);
            // 
            // rbResetBranch
            // 
            this.rbResetBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rbResetBranch.AutoSize = true;
            this.rbResetBranch.Checked = true;
            this.rbResetBranch.Location = new System.Drawing.Point(4, 4);
            this.rbResetBranch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbResetBranch.Name = "rbResetBranch";
            this.rbResetBranch.Size = new System.Drawing.Size(236, 21);
            this.rbResetBranch.TabIndex = 0;
            this.rbResetBranch.TabStop = true;
            this.rbResetBranch.Text = "Reset local branch with the name:";
            this.rbResetBranch.UseVisualStyleBackColor = true;
            // 
            // rbCreateBranchWithCustomName
            // 
            this.rbCreateBranchWithCustomName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rbCreateBranchWithCustomName.AutoSize = true;
            this.rbCreateBranchWithCustomName.Location = new System.Drawing.Point(4, 33);
            this.rbCreateBranchWithCustomName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 5);
            this.rbCreateBranchWithCustomName.Name = "rbCreateBranchWithCustomName";
            this.rbCreateBranchWithCustomName.Size = new System.Drawing.Size(268, 21);
            this.rbCreateBranchWithCustomName.TabIndex = 2;
            this.rbCreateBranchWithCustomName.Text = "Create local branch with custom name:";
            this.rbCreateBranchWithCustomName.UseVisualStyleBackColor = true;
            this.rbCreateBranchWithCustomName.CheckedChanged += new System.EventHandler(this.rbCreateBranchWithCustomName_CheckedChanged);
            // 
            // branchName
            // 
            this.branchName.AutoEllipsis = true;
            this.branchName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchName.Location = new System.Drawing.Point(280, 0);
            this.branchName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.branchName.Name = "branchName";
            this.branchName.Size = new System.Drawing.Size(299, 29);
            this.branchName.TabIndex = 24;
            this.branchName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.setBranchPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.horLine, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.remoteOptionsPanel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.localChangesPanel, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(9, 9, 9, 9);
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(610, 269);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.TabStop = true;
            // 
            // FormCheckoutBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(621, 294);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimizeBox = false;
            this.Name = "FormCheckoutBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout branch";
            this.Activated += new System.EventHandler(this.FormCheckoutBranch_Activated);
            this.setBranchPanel.ResumeLayout(false);
            this.setBranchPanel.PerformLayout();
            this.localChangesPanel.ResumeLayout(false);
            this.localChangesPanel.PerformLayout();
            this.localChangesGB.ResumeLayout(false);
            this.localChangesGB.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.remoteOptionsPanel.ResumeLayout(false);
            this.remoteOptionsPanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label horLine;
        private System.Windows.Forms.TableLayoutPanel setBranchPanel;
        private System.Windows.Forms.TableLayoutPanel localChangesPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel remoteOptionsPanel;
        private System.Windows.Forms.RadioButton rbDontCreate;
        private System.Windows.Forms.TextBox txtCustomBranchName;
        private System.Windows.Forms.RadioButton rbResetBranch;
        private System.Windows.Forms.RadioButton rbCreateBranchWithCustomName;
        private System.Windows.Forms.Label branchName;
        private System.Windows.Forms.GroupBox localChangesGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton rbReset;
        private System.Windows.Forms.RadioButton rbStash;
        private System.Windows.Forms.RadioButton rbMerge;
        private System.Windows.Forms.RadioButton rbDontChange;
        private System.Windows.Forms.RadioButton LocalBranch;
        private System.Windows.Forms.RadioButton Remotebranch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbChanges;
        private System.Windows.Forms.CheckBox chkSetLocalChangesActionAsDefault;
    }
}
