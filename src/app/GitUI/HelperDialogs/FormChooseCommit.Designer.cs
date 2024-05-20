namespace GitUI.HelperDialogs
{
    partial class FormChooseCommit
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
            btnOK = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            revisionGrid = new GitUI.RevisionGridControl();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            buttonGotoCommit = new Button();
            flowLayoutPanelParents = new FlowLayoutPanel();
            labelParents = new Label();
            linkLabelParent = new LinkLabel();
            linkLabelParent2 = new LinkLabel();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanelParents.SuspendLayout();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(963, 480);
            btnOK.Margin = new Padding(4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(138, 31);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35.85699F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64.14301F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 153F));
            tableLayoutPanel1.Controls.Add(revisionGrid, 0, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel1.Controls.Add(btnOK, 2, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanelParents, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.Size = new Size(1105, 515);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // revisionGrid
            // 
            revisionGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.SetColumnSpan(revisionGrid, 3);
            revisionGrid.DoubleClickDoesNotOpenCommitInfo = true;
            revisionGrid.Location = new Point(4, 4);
            revisionGrid.Margin = new Padding(4);
            revisionGrid.MinimumSize = new Size(250, 125);
            revisionGrid.Name = "revisionGrid";
            revisionGrid.Size = new Size(1097, 466);
            revisionGrid.TabIndex = 0;
            revisionGrid.DoubleClickRevision += new System.EventHandler<GitUI.UserControls.RevisionGrid.DoubleClickRevisionEventArgs>(revisionGrid_DoubleClickRevision);
            revisionGrid.SelectionChanged += revisionGrid_SelectionChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(buttonGotoCommit);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(4, 478);
            flowLayoutPanel1.Margin = new Padding(4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(333, 33);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 9);
            label1.Margin = new Padding(4, 9, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(135, 17);
            label1.TabIndex = 4;
            label1.Text = "Find specific commit:";
            // 
            // buttonGotoCommit
            // 
            buttonGotoCommit.Location = new Point(147, 4);
            buttonGotoCommit.Margin = new Padding(4);
            buttonGotoCommit.Name = "buttonGotoCommit";
            buttonGotoCommit.Size = new Size(162, 29);
            buttonGotoCommit.TabIndex = 3;
            buttonGotoCommit.Text = "Go to commit...";
            buttonGotoCommit.UseVisualStyleBackColor = true;
            buttonGotoCommit.Click += buttonGotoCommit_Click;
            // 
            // flowLayoutPanelParents
            // 
            flowLayoutPanelParents.Controls.Add(labelParents);
            flowLayoutPanelParents.Controls.Add(linkLabelParent);
            flowLayoutPanelParents.Controls.Add(linkLabelParent2);
            flowLayoutPanelParents.Dock = DockStyle.Fill;
            flowLayoutPanelParents.Location = new Point(344, 477);
            flowLayoutPanelParents.Name = "flowLayoutPanelParents";
            flowLayoutPanelParents.Size = new Size(604, 35);
            flowLayoutPanelParents.TabIndex = 3;
            // 
            // labelParents
            // 
            labelParents.AutoSize = true;
            labelParents.Location = new Point(4, 9);
            labelParents.Margin = new Padding(4, 9, 4, 0);
            labelParents.Name = "labelParents";
            labelParents.Size = new Size(69, 17);
            labelParents.TabIndex = 0;
            labelParents.Text = "Parent(s):";
            labelParents.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // linkLabelParent
            // 
            linkLabelParent.AutoSize = true;
            linkLabelParent.Location = new Point(81, 9);
            linkLabelParent.Margin = new Padding(4, 9, 4, 0);
            linkLabelParent.Name = "linkLabelParent";
            linkLabelParent.Size = new Size(85, 17);
            linkLabelParent.TabIndex = 1;
            linkLabelParent.TabStop = true;
            linkLabelParent.Text = "sha parent 1";
            linkLabelParent.LinkClicked += linkLabelParent_LinkClicked;
            // 
            // linkLabelParent2
            // 
            linkLabelParent2.AutoSize = true;
            linkLabelParent2.Location = new Point(174, 9);
            linkLabelParent2.Margin = new Padding(4, 9, 4, 0);
            linkLabelParent2.Name = "linkLabelParent2";
            linkLabelParent2.Size = new Size(85, 17);
            linkLabelParent2.TabIndex = 1;
            linkLabelParent2.TabStop = true;
            linkLabelParent2.Text = "sha parent 2";
            linkLabelParent2.LinkClicked += linkLabelParent_LinkClicked;
            // 
            // FormChooseCommit
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(120F, 120F);
            ClientSize = new Size(1105, 515);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4);
            Name = "FormChooseCommit";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Choose Commit";
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanelParents.ResumeLayout(false);
            flowLayoutPanelParents.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private RevisionGridControl revisionGrid;
        private Button btnOK;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label1;
        private Button buttonGotoCommit;
        private FlowLayoutPanel flowLayoutPanelParents;
        private Label labelParents;
        private LinkLabel linkLabelParent;
        private LinkLabel linkLabelParent2;
    }
}