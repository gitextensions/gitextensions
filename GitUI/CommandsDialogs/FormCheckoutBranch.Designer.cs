
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
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.setBranchPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LocalBranch = new System.Windows.Forms.RadioButton();
            this.Remotebranch = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.lbChanges = new System.Windows.Forms.Label();
            this.horLine = new System.Windows.Forms.Label();
            this.localChangesPanel = new System.Windows.Forms.TableLayoutPanel();
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.rbDontChange = new System.Windows.Forms.RadioButton();
            this.rbMerge = new System.Windows.Forms.RadioButton();
            this.rbStash = new System.Windows.Forms.RadioButton();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.chkSetLocalChangesActionAsDefault = new System.Windows.Forms.CheckBox();
            this.remoteOptionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rbDontCreate = new System.Windows.Forms.RadioButton();
            this.txtCustomBranchName = new System.Windows.Forms.TextBox();
            this.rbResetBranch = new System.Windows.Forms.RadioButton();
            this.rbCreateBranchWithCustomName = new System.Windows.Forms.RadioButton();
            this.branchName = new System.Windows.Forms.Label();
            this.Errors = new System.Windows.Forms.ErrorProvider(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.setBranchPanel.SuspendLayout();
            this.localChangesPanel.SuspendLayout();
            this.localChangesGB.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.remoteOptionsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Errors)).BeginInit();
            this.SuspendLayout();
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
            this.Ok.Location = new System.Drawing.Point(480, 20);
            this.Ok.Margin = new System.Windows.Forms.Padding(0, 20, 6, 6);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(62, 23);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Checkout";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.setBranchPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.horLine, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.localChangesPanel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.remoteOptionsPanel, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(7);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(565, 209);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.TabStop = true;
            // 
            // setBranchPanel
            // 
            this.setBranchPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.setBranchPanel.AutoSize = true;
            this.setBranchPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.setBranchPanel.ColumnCount = 3;
            this.setBranchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.setBranchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.setBranchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.setBranchPanel.Controls.Add(this.LocalBranch, 0, 0);
            this.setBranchPanel.Controls.Add(this.Remotebranch, 1, 0);
            this.setBranchPanel.Controls.Add(this.label1, 0, 1);
            this.setBranchPanel.Controls.Add(this.Branches, 1, 1);
            this.setBranchPanel.Controls.Add(this.lbChanges, 2, 1);
            this.setBranchPanel.Location = new System.Drawing.Point(10, 10);
            this.setBranchPanel.Name = "setBranchPanel";
            this.setBranchPanel.RowCount = 2;
            this.setBranchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.setBranchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.setBranchPanel.Size = new System.Drawing.Size(545, 48);
            this.setBranchPanel.TabIndex = 0;
            // 
            // LocalBranch
            // 
            this.LocalBranch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LocalBranch.AutoSize = true;
            this.LocalBranch.CausesValidation = false;
            this.LocalBranch.Checked = true;
            this.LocalBranch.Location = new System.Drawing.Point(2, 2);
            this.LocalBranch.Margin = new System.Windows.Forms.Padding(2);
            this.LocalBranch.Name = "LocalBranch";
            this.LocalBranch.Size = new System.Drawing.Size(85, 17);
            this.LocalBranch.TabIndex = 0;
            this.LocalBranch.TabStop = true;
            this.LocalBranch.Text = "Local branch";
            this.LocalBranch.UseVisualStyleBackColor = true;
            this.LocalBranch.CheckedChanged += new System.EventHandler(this.LocalBranchCheckedChanged);
            // 
            // Remotebranch
            // 
            this.Remotebranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Remotebranch.AutoSize = true;
            this.Remotebranch.Location = new System.Drawing.Point(91, 2);
            this.Remotebranch.CausesValidation = false;
            this.Remotebranch.Margin = new System.Windows.Forms.Padding(2);
            this.Remotebranch.Name = "Remotebranch";
            this.Remotebranch.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.Remotebranch.Size = new System.Drawing.Size(104, 17);
            this.Remotebranch.TabIndex = 1;
            this.Remotebranch.Text = "Remote branch";
            this.Remotebranch.UseVisualStyleBackColor = true;
            this.Remotebranch.CheckedChanged += new System.EventHandler(this.RemoteBranchCheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 27);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select branch";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(92, 24);
            this.Errors.SetIconAlignment(this.Branches, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(433, 21);
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
            this.lbChanges.Location = new System.Drawing.Point(531, 21);
            this.lbChanges.Name = "lbChanges";
            this.lbChanges.Size = new System.Drawing.Size(11, 27);
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
            this.horLine.Location = new System.Drawing.Point(10, 64);
            this.horLine.Margin = new System.Windows.Forms.Padding(3);
            this.horLine.Name = "horLine";
            this.horLine.Size = new System.Drawing.Size(545, 2);
            this.horLine.TabIndex = 1;
            // 
            // localChangesPanel
            // 
            this.localChangesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.localChangesPanel.ColumnCount = 2;
            this.localChangesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.localChangesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 71F));
            this.localChangesPanel.Controls.Add(this.localChangesGB, 0, 0);
            this.localChangesPanel.Controls.Add(this.Ok, 1, 0);
            this.localChangesPanel.Location = new System.Drawing.Point(7, 146);
            this.localChangesPanel.Margin = new System.Windows.Forms.Padding(0);
            this.localChangesPanel.Name = "localChangesPanel";
            this.localChangesPanel.RowCount = 1;
            this.localChangesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.localChangesPanel.Size = new System.Drawing.Size(551, 58);
            this.localChangesPanel.TabIndex = 3;
            // 
            // localChangesGB
            // 
            this.localChangesGB.Controls.Add(this.flowLayoutPanel2);
            this.localChangesGB.Location = new System.Drawing.Point(0, 0);
            this.localChangesGB.Margin = new System.Windows.Forms.Padding(0);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Size = new System.Drawing.Size(444, 54);
            this.localChangesGB.TabIndex = 0;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.rbDontChange);
            this.flowLayoutPanel2.Controls.Add(this.rbMerge);
            this.flowLayoutPanel2.Controls.Add(this.rbStash);
            this.flowLayoutPanel2.Controls.Add(this.rbReset);
            this.flowLayoutPanel2.Controls.Add(this.chkSetLocalChangesActionAsDefault);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(6, 20);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(424, 26);
            this.flowLayoutPanel2.TabIndex = 1;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // rbDontChange
            // 
            this.rbDontChange.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbDontChange.AutoSize = true;
            this.rbDontChange.Location = new System.Drawing.Point(2, 2);
            this.rbDontChange.Margin = new System.Windows.Forms.Padding(2);
            this.rbDontChange.Name = "rbDontChange";
            this.rbDontChange.Size = new System.Drawing.Size(88, 19);
            this.rbDontChange.TabIndex = 0;
            this.rbDontChange.Text = "Don\'t change";
            this.rbDontChange.UseVisualStyleBackColor = false;
            // 
            // rbMerge
            // 
            this.rbMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbMerge.AutoSize = true;
            this.rbMerge.Location = new System.Drawing.Point(94, 2);
            this.rbMerge.Margin = new System.Windows.Forms.Padding(2);
            this.rbMerge.Name = "rbMerge";
            this.rbMerge.Size = new System.Drawing.Size(55, 19);
            this.rbMerge.TabIndex = 1;
            this.rbMerge.Text = "Merge";
            this.rbMerge.UseVisualStyleBackColor = false;
            // 
            // rbStash
            // 
            this.rbStash.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbStash.AutoSize = true;
            this.rbStash.Location = new System.Drawing.Point(153, 2);
            this.rbStash.Margin = new System.Windows.Forms.Padding(2);
            this.rbStash.Name = "rbStash";
            this.rbStash.Size = new System.Drawing.Size(52, 19);
            this.rbStash.TabIndex = 2;
            this.rbStash.Text = "Stash";
            this.rbStash.UseVisualStyleBackColor = false;
            // 
            // rbReset
            // 
            this.rbReset.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(209, 2);
            this.rbReset.Margin = new System.Windows.Forms.Padding(2);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(53, 19);
            this.rbReset.TabIndex = 3;
            this.rbReset.Text = "Reset";
            this.rbReset.UseVisualStyleBackColor = false;
            this.rbReset.CheckedChanged += new System.EventHandler(this.rbReset_CheckedChanged);
            // 
            // chkSetLocalChangesActionAsDefault
            // 
            this.chkSetLocalChangesActionAsDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSetLocalChangesActionAsDefault.AutoSize = true;
            this.chkSetLocalChangesActionAsDefault.Location = new System.Drawing.Point(267, 3);
            this.chkSetLocalChangesActionAsDefault.Name = "chkSetLocalChangesActionAsDefault";
            this.chkSetLocalChangesActionAsDefault.Size = new System.Drawing.Size(93, 17);
            this.chkSetLocalChangesActionAsDefault.TabIndex = 4;
            this.chkSetLocalChangesActionAsDefault.Text = "Set as default";
            this.chkSetLocalChangesActionAsDefault.UseVisualStyleBackColor = false;
            // 
            // remoteOptionsPanel
            // 
            this.remoteOptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remoteOptionsPanel.AutoSize = true;
            this.remoteOptionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.remoteOptionsPanel.ColumnCount = 2;
            this.remoteOptionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.remoteOptionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.remoteOptionsPanel.Controls.Add(this.rbDontCreate, 0, 4);
            this.remoteOptionsPanel.Controls.Add(this.txtCustomBranchName, 1, 2);
            this.remoteOptionsPanel.Controls.Add(this.rbResetBranch, 0, 0);
            this.remoteOptionsPanel.Controls.Add(this.rbCreateBranchWithCustomName, 0, 2);
            this.remoteOptionsPanel.Controls.Add(this.branchName, 1, 0);
            this.remoteOptionsPanel.Location = new System.Drawing.Point(10, 72);
            this.remoteOptionsPanel.Name = "remoteOptionsPanel";
            this.remoteOptionsPanel.RowCount = 5;
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.remoteOptionsPanel.Size = new System.Drawing.Size(545, 71);
            this.remoteOptionsPanel.TabIndex = 2;
            // 
            // rbDontCreate
            // 
            this.rbDontCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbDontCreate.AutoSize = true;
            this.rbDontCreate.Location = new System.Drawing.Point(3, 51);
            this.rbDontCreate.Name = "rbDontCreate";
            this.rbDontCreate.Size = new System.Drawing.Size(211, 17);
            this.rbDontCreate.TabIndex = 4;
            this.rbDontCreate.Text = "Checkout remote branch";
            this.rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // txtCustomBranchName
            // 
            this.txtCustomBranchName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCustomBranchName.Enabled = false;
            this.txtCustomBranchName.Location = new System.Drawing.Point(219, 25);
            this.txtCustomBranchName.Margin = new System.Windows.Forms.Padding(2);
            this.txtCustomBranchName.Name = "txtCustomBranchName";
            this.txtCustomBranchName.Size = new System.Drawing.Size(324, 21);
            this.txtCustomBranchName.TabIndex = 3;
            this.txtCustomBranchName.Leave += new System.EventHandler(this.txtCustomBranchName_Leave);
            // 
            // rbResetBranch
            // 
            this.rbResetBranch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbResetBranch.AutoSize = true;
            this.rbResetBranch.Checked = true;
            this.rbResetBranch.Location = new System.Drawing.Point(3, 3);
            this.rbResetBranch.Name = "rbResetBranch";
            this.rbResetBranch.Size = new System.Drawing.Size(211, 17);
            this.rbResetBranch.TabIndex = 0;
            this.rbResetBranch.TabStop = true;
            this.rbResetBranch.Text = "Reset local branch with the name:";
            this.rbResetBranch.UseVisualStyleBackColor = true;
            // 
            // rbCreateBranchWithCustomName
            // 
            this.rbCreateBranchWithCustomName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rbCreateBranchWithCustomName.AutoSize = true;
            this.rbCreateBranchWithCustomName.Location = new System.Drawing.Point(3, 26);
            this.rbCreateBranchWithCustomName.Margin = new System.Windows.Forms.Padding(3, 3, 3, 4);
            this.rbCreateBranchWithCustomName.Name = "rbCreateBranchWithCustomName";
            this.rbCreateBranchWithCustomName.Size = new System.Drawing.Size(211, 18);
            this.rbCreateBranchWithCustomName.TabIndex = 2;
            this.rbCreateBranchWithCustomName.Text = "Create local branch with custom name:";
            this.rbCreateBranchWithCustomName.UseVisualStyleBackColor = true;
            this.rbCreateBranchWithCustomName.CheckedChanged += new System.EventHandler(this.rbCreateBranchWithCustomName_CheckedChanged);
            // 
            // branchName
            // 
            this.branchName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.branchName.AutoEllipsis = true;
            this.branchName.Location = new System.Drawing.Point(220, 0);
            this.branchName.Name = "branchName";
            this.branchName.Size = new System.Drawing.Size(322, 23);
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
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(573, 221);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.MinimizeBox = false;
            this.Name = "FormCheckoutBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout branch";
            this.Activated += new System.EventHandler(this.FormCheckoutBranch_Activated);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.setBranchPanel.ResumeLayout(false);
            this.setBranchPanel.PerformLayout();
            this.localChangesPanel.ResumeLayout(false);
            this.localChangesPanel.PerformLayout();
            this.localChangesGB.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.remoteOptionsPanel.ResumeLayout(false);
            this.remoteOptionsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Errors)).EndInit();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.RadioButton rbReset;
        private System.Windows.Forms.RadioButton rbStash;
        private System.Windows.Forms.RadioButton rbMerge;
        private System.Windows.Forms.RadioButton rbDontChange;
        private System.Windows.Forms.RadioButton LocalBranch;
        private System.Windows.Forms.RadioButton Remotebranch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbChanges;
        private System.Windows.Forms.CheckBox chkSetLocalChangesActionAsDefault;
        private FlowLayoutPanel flowLayoutPanel2;
        private ErrorProvider Errors;
    }
}
