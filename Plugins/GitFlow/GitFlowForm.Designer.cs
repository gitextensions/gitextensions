namespace GitExtensions.Plugins.GitFlow
{
    partial class GitFlowForm
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
            btnInit = new Button();
            txtBranchName = new TextBox();
            btnCreateBranch = new Button();
            cbBranches = new ComboBox();
            btnFinish = new Button();
            gbManage = new GroupBox();
            pnlManageBranch = new Panel();
            cbSquash = new CheckBox();
            cbPushAfterFinish = new CheckBox();
            panel2 = new Panel();
            pnlPull = new Panel();
            cbRemote = new ComboBox();
            label9 = new Label();
            btnPull = new Button();
            panel1 = new Panel();
            lblPrefixManage = new Label();
            label1 = new Label();
            btnPublish = new Button();
            cbManageType = new ComboBox();
            btnClose = new Button();
            cbType = new ComboBox();
            label10 = new Label();
            lblPrefixName = new Label();
            gbStart = new GroupBox();
            pnlBasedOn = new Panel();
            cbBaseBranch = new ComboBox();
            cbBasedOn = new CheckBox();
            lblDebug = new Label();
            lnkGitFlow = new LinkLabel();
            pbResultCommand = new PictureBox();
            ttGitFlow = new ToolTip(components);
            ttCommandResult = new ToolTip(components);
            ttDebug = new ToolTip(components);
            lblCaptionHead = new Label();
            lblHead = new Label();
            label2 = new Label();
            panel3 = new GroupBox();
            txtResult = new TextBox();
            lblRunCommand = new Label();
            gbManage.SuspendLayout();
            pnlManageBranch.SuspendLayout();
            pnlPull.SuspendLayout();
            gbStart.SuspendLayout();
            pnlBasedOn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pbResultCommand)).BeginInit();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // btnInit
            // 
            btnInit.Location = new Point(284, 12);
            btnInit.Name = "btnInit";
            btnInit.Size = new Size(130, 23);
            btnInit.TabIndex = 0;
            btnInit.Text = "Init GitFlow";
            btnInit.UseVisualStyleBackColor = true;
            btnInit.Click += btnInit_Click;
            // 
            // txtBranchName
            // 
            txtBranchName.Location = new Point(163, 25);
            txtBranchName.Name = "txtBranchName";
            txtBranchName.Size = new Size(374, 20);
            txtBranchName.TabIndex = 2;
            // 
            // btnCreateBranch
            // 
            btnCreateBranch.Location = new Point(542, 23);
            btnCreateBranch.Name = "btnCreateBranch";
            btnCreateBranch.Size = new Size(81, 23);
            btnCreateBranch.TabIndex = 0;
            btnCreateBranch.Text = "Start!";
            btnCreateBranch.UseVisualStyleBackColor = true;
            btnCreateBranch.Click += btnStartBranch_Click;
            // 
            // cbBranches
            // 
            cbBranches.DropDownStyle = ComboBoxStyle.DropDownList;
            cbBranches.FormattingEnabled = true;
            cbBranches.Location = new Point(188, 8);
            cbBranches.Name = "cbBranches";
            cbBranches.Size = new Size(374, 21);
            cbBranches.TabIndex = 3;
            // 
            // btnFinish
            // 
            btnFinish.Location = new Point(490, 72);
            btnFinish.Name = "btnFinish";
            btnFinish.Size = new Size(82, 23);
            btnFinish.TabIndex = 0;
            btnFinish.Text = "Finish";
            btnFinish.UseVisualStyleBackColor = true;
            btnFinish.Click += btnFinish_Click;
            // 
            // gbManage
            // 
            gbManage.Controls.Add(pnlManageBranch);
            gbManage.Controls.Add(cbManageType);
            gbManage.Location = new Point(13, 149);
            gbManage.Name = "gbManage";
            gbManage.Size = new Size(628, 162);
            gbManage.TabIndex = 6;
            gbManage.TabStop = false;
            gbManage.Text = "Manage existing branches:";
            // 
            // pnlManageBranch
            // 
            pnlManageBranch.Controls.Add(cbSquash);
            pnlManageBranch.Controls.Add(cbPushAfterFinish);
            pnlManageBranch.Controls.Add(panel2);
            pnlManageBranch.Controls.Add(pnlPull);
            pnlManageBranch.Controls.Add(panel1);
            pnlManageBranch.Controls.Add(lblPrefixManage);
            pnlManageBranch.Controls.Add(label1);
            pnlManageBranch.Controls.Add(cbBranches);
            pnlManageBranch.Controls.Add(btnPublish);
            pnlManageBranch.Controls.Add(btnFinish);
            pnlManageBranch.Location = new Point(10, 30);
            pnlManageBranch.Name = "pnlManageBranch";
            pnlManageBranch.Size = new Size(610, 131);
            pnlManageBranch.TabIndex = 7;
            // 
            // cbSquash
            // 
            cbSquash.AutoSize = true;
            cbSquash.Location = new Point(492, 113);
            cbSquash.Name = "cbSquash";
            cbSquash.Size = new Size(62, 17);
            cbSquash.TabIndex = 9;
            cbSquash.Text = "Squash";
            cbSquash.UseVisualStyleBackColor = true;
            // 
            // cbPushAfterFinish
            // 
            cbPushAfterFinish.AutoSize = true;
            cbPushAfterFinish.Location = new Point(492, 97);
            cbPushAfterFinish.Name = "cbPushAfterFinish";
            cbPushAfterFinish.Size = new Size(101, 17);
            cbPushAfterFinish.TabIndex = 8;
            cbPushAfterFinish.Text = "Push after finish";
            cbPushAfterFinish.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ActiveCaption;
            panel2.Location = new Point(448, 54);
            panel2.Name = "panel2";
            panel2.Size = new Size(1, 70);
            panel2.TabIndex = 5;
            // 
            // pnlPull
            // 
            pnlPull.Controls.Add(cbRemote);
            pnlPull.Controls.Add(label9);
            pnlPull.Controls.Add(btnPull);
            pnlPull.Location = new Point(168, 40);
            pnlPull.Name = "pnlPull";
            pnlPull.Size = new Size(274, 84);
            pnlPull.TabIndex = 6;
            // 
            // cbRemote
            // 
            cbRemote.DropDownStyle = ComboBoxStyle.DropDownList;
            cbRemote.FormattingEnabled = true;
            cbRemote.Location = new Point(12, 24);
            cbRemote.Name = "cbRemote";
            cbRemote.Size = new Size(239, 21);
            cbRemote.TabIndex = 3;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(80, 9);
            label9.Name = "label9";
            label9.Size = new Size(104, 13);
            label9.TabIndex = 1;
            label9.Text = "Remote to pull from :";
            // 
            // btnPull
            // 
            btnPull.Location = new Point(96, 47);
            btnPull.Name = "btnPull";
            btnPull.Size = new Size(83, 23);
            btnPull.TabIndex = 0;
            btnPull.Text = "Pull";
            btnPull.UseVisualStyleBackColor = true;
            btnPull.Click += btnPull_Click;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(151, 54);
            panel1.Name = "panel1";
            panel1.Size = new Size(1, 70);
            panel1.TabIndex = 4;
            // 
            // lblPrefixManage
            // 
            lblPrefixManage.AutoSize = true;
            lblPrefixManage.Location = new Point(104, 11);
            lblPrefixManage.Name = "lblPrefixManage";
            lblPrefixManage.Size = new Size(43, 13);
            lblPrefixManage.TabIndex = 1;
            lblPrefixManage.Text = "[prefix]/";
            lblPrefixManage.TextAlign = ContentAlignment.TopRight;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(38, 11);
            label1.Name = "label1";
            label1.Size = new Size(43, 13);
            label1.TabIndex = 1;
            label1.Text = "branch:";
            // 
            // btnPublish
            // 
            btnPublish.Location = new Point(31, 72);
            btnPublish.Name = "btnPublish";
            btnPublish.Size = new Size(81, 23);
            btnPublish.TabIndex = 0;
            btnPublish.Text = "Publish";
            btnPublish.UseVisualStyleBackColor = true;
            btnPublish.Click += btnPublish_Click;
            // 
            // cbManageType
            // 
            cbManageType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbManageType.FormattingEnabled = true;
            cbManageType.Location = new Point(197, 0);
            cbManageType.Name = "cbManageType";
            cbManageType.Size = new Size(181, 21);
            cbManageType.TabIndex = 3;
            cbManageType.SelectedValueChanged += cbManageType_SelectedValueChanged;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(287, 525);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(83, 23);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // cbType
            // 
            cbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbType.FormattingEnabled = true;
            cbType.Location = new Point(118, 0);
            cbType.Name = "cbType";
            cbType.Size = new Size(181, 21);
            cbType.TabIndex = 3;
            cbType.SelectedValueChanged += cbType_SelectedValueChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(9, 28);
            label10.Name = "label10";
            label10.Size = new Size(84, 13);
            label10.TabIndex = 1;
            label10.Text = "Expected name:";
            // 
            // lblPrefixName
            // 
            lblPrefixName.AutoSize = true;
            lblPrefixName.Location = new Point(119, 28);
            lblPrefixName.Name = "lblPrefixName";
            lblPrefixName.Size = new Size(43, 13);
            lblPrefixName.TabIndex = 1;
            lblPrefixName.Text = "[prefix]/";
            lblPrefixName.TextAlign = ContentAlignment.TopRight;
            // 
            // gbStart
            // 
            gbStart.Controls.Add(pnlBasedOn);
            gbStart.Controls.Add(cbType);
            gbStart.Controls.Add(txtBranchName);
            gbStart.Controls.Add(lblPrefixName);
            gbStart.Controls.Add(label10);
            gbStart.Controls.Add(btnCreateBranch);
            gbStart.Location = new Point(12, 41);
            gbStart.Name = "gbStart";
            gbStart.Size = new Size(629, 100);
            gbStart.TabIndex = 8;
            gbStart.TabStop = false;
            gbStart.Text = "Start branch:";
            // 
            // pnlBasedOn
            // 
            pnlBasedOn.Controls.Add(cbBaseBranch);
            pnlBasedOn.Controls.Add(cbBasedOn);
            pnlBasedOn.Location = new Point(119, 51);
            pnlBasedOn.Name = "pnlBasedOn";
            pnlBasedOn.Size = new Size(501, 37);
            pnlBasedOn.TabIndex = 4;
            // 
            // cbBaseBranch
            // 
            cbBaseBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cbBaseBranch.FormattingEnabled = true;
            cbBaseBranch.Location = new Point(125, 8);
            cbBaseBranch.Name = "cbBaseBranch";
            cbBaseBranch.Size = new Size(340, 21);
            cbBaseBranch.TabIndex = 3;
            // 
            // cbBasedOn
            // 
            cbBasedOn.AutoSize = true;
            cbBasedOn.Location = new Point(3, 11);
            cbBasedOn.Name = "cbBasedOn";
            cbBasedOn.Size = new Size(73, 17);
            cbBasedOn.TabIndex = 0;
            cbBasedOn.Text = "based on:";
            cbBasedOn.UseVisualStyleBackColor = true;
            cbBasedOn.CheckedChanged += cbBasedOn_CheckedChanged;
            // 
            // lblDebug
            // 
            lblDebug.AutoSize = true;
            lblDebug.Location = new Point(24, 530);
            lblDebug.Name = "lblDebug";
            lblDebug.Size = new Size(238, 13);
            lblDebug.TabIndex = 7;
            lblDebug.Text = "                                                                             ";
            // 
            // lnkGitFlow
            // 
            lnkGitFlow.AutoSize = true;
            lnkGitFlow.Location = new Point(570, 17);
            lnkGitFlow.Name = "lnkGitFlow";
            lnkGitFlow.Size = new Size(73, 13);
            lnkGitFlow.TabIndex = 9;
            lnkGitFlow.TabStop = true;
            lnkGitFlow.Text = "About GitFlow";
            lnkGitFlow.LinkClicked += lnkGitFlow_LinkClicked;
            // 
            // pbResultCommand
            // 
            pbResultCommand.Location = new Point(12, 10);
            pbResultCommand.Name = "pbResultCommand";
            pbResultCommand.Size = new Size(25, 25);
            pbResultCommand.SizeMode = PictureBoxSizeMode.StretchImage;
            pbResultCommand.TabIndex = 10;
            pbResultCommand.TabStop = false;
            // 
            // ttGitFlow
            // 
            ttGitFlow.AutoPopDelay = 10000;
            ttGitFlow.InitialDelay = 0;
            ttGitFlow.ReshowDelay = 0;
            // 
            // ttCommandResult
            // 
            ttCommandResult.AutoPopDelay = 32000;
            ttCommandResult.InitialDelay = 0;
            ttCommandResult.ReshowDelay = 0;
            ttCommandResult.ShowAlways = true;
            // 
            // ttDebug
            // 
            ttDebug.AutoPopDelay = 32000;
            ttDebug.InitialDelay = 0;
            ttDebug.ReshowDelay = 0;
            ttDebug.ShowAlways = true;
            // 
            // lblCaptionHead
            // 
            lblCaptionHead.AutoSize = true;
            lblCaptionHead.Location = new Point(176, 17);
            lblCaptionHead.Name = "lblCaptionHead";
            lblCaptionHead.Size = new Size(40, 13);
            lblCaptionHead.TabIndex = 1;
            lblCaptionHead.Text = "HEAD:";
            // 
            // lblHead
            // 
            lblHead.AutoSize = true;
            lblHead.Location = new Point(217, 17);
            lblHead.Name = "lblHead";
            lblHead.Size = new Size(33, 13);
            lblHead.TabIndex = 1;
            lblHead.Text = "ref/...";
            lblHead.TextAlign = ContentAlignment.TopRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 14);
            label2.Name = "label2";
            label2.Size = new Size(56, 13);
            label2.TabIndex = 1;
            label2.Text = "command:";
            // 
            // panel3
            // 
            panel3.Controls.Add(txtResult);
            panel3.Controls.Add(lblRunCommand);
            panel3.Controls.Add(label2);
            panel3.Location = new Point(12, 319);
            panel3.Name = "panel3";
            panel3.Size = new Size(629, 200);
            panel3.TabIndex = 11;
            panel3.TabStop = false;
            panel3.Text = "Result of git flow command run";
            // 
            // txtResult
            // 
            txtResult.AcceptsReturn = true;
            txtResult.BorderStyle = BorderStyle.None;
            txtResult.Location = new Point(6, 32);
            txtResult.Multiline = true;
            txtResult.Name = "txtResult";
            txtResult.ReadOnly = true;
            txtResult.ScrollBars = ScrollBars.Both;
            txtResult.Size = new Size(617, 162);
            txtResult.TabIndex = 2;
            txtResult.Text = " -";
            // 
            // lblRunCommand
            // 
            lblRunCommand.AutoSize = true;
            lblRunCommand.Location = new Point(60, 14);
            lblRunCommand.Name = "lblRunCommand";
            lblRunCommand.Size = new Size(10, 13);
            lblRunCommand.TabIndex = 1;
            lblRunCommand.Text = "-";
            // 
            // GitFlowForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(653, 556);
            Controls.Add(panel3);
            Controls.Add(pbResultCommand);
            Controls.Add(lnkGitFlow);
            Controls.Add(gbStart);
            Controls.Add(lblHead);
            Controls.Add(lblDebug);
            Controls.Add(lblCaptionHead);
            Controls.Add(gbManage);
            Controls.Add(btnClose);
            Controls.Add(btnInit);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "GitFlowForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "GitFlow";
            gbManage.ResumeLayout(false);
            pnlManageBranch.ResumeLayout(false);
            pnlManageBranch.PerformLayout();
            pnlPull.ResumeLayout(false);
            pnlPull.PerformLayout();
            gbStart.ResumeLayout(false);
            gbStart.PerformLayout();
            pnlBasedOn.ResumeLayout(false);
            pnlBasedOn.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pbResultCommand)).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button btnInit;
        private TextBox txtBranchName;
        private Button btnCreateBranch;
        private ComboBox cbBranches;
        private Button btnFinish;
        private GroupBox gbManage;
        private Button btnClose;
        private ComboBox cbRemote;
        private Label label9;
        private Button btnPull;
        private Button btnPublish;
        private ComboBox cbType;
        private Label label10;
        private Label lblPrefixName;
        private GroupBox gbStart;
        private ComboBox cbManageType;
        private Panel panel1;
        private Label label1;
        private Label lblPrefixManage;
        private Panel panel2;
        private Panel pnlPull;
        private Panel pnlManageBranch;
        private Panel pnlBasedOn;
        private ComboBox cbBaseBranch;
        private CheckBox cbBasedOn;
        private Label lblDebug;
        private LinkLabel lnkGitFlow;
        private PictureBox pbResultCommand;
        private ToolTip ttGitFlow;
        private ToolTip ttCommandResult;
        private ToolTip ttDebug;
        private Label lblCaptionHead;
        private Label lblHead;
        private Label label2;
        private GroupBox panel3;
        private Label lblRunCommand;
        private TextBox txtResult;
        private CheckBox cbPushAfterFinish;
        private CheckBox cbSquash;
    }
}
