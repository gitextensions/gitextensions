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
            this.btnOK = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.revisionGrid = new GitUI.RevisionGridControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonGotoCommit = new System.Windows.Forms.Button();
            this.flowLayoutPanelParents = new System.Windows.Forms.FlowLayoutPanel();
            this.labelParents = new System.Windows.Forms.Label();
            this.linkLabelParent = new System.Windows.Forms.LinkLabel();
            this.linkLabelParent2 = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanelParents.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(963, 480);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(138, 31);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.85699F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64.14301F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.tableLayoutPanel1.Controls.Add(this.revisionGrid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnOK, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanelParents, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1105, 515);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // revisionGrid
            // 
            this.revisionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.revisionGrid, 3);
            this.revisionGrid.DoubleClickDoesNotOpenCommitInfo = true;
            this.revisionGrid.Location = new System.Drawing.Point(4, 4);
            this.revisionGrid.Margin = new System.Windows.Forms.Padding(4);
            this.revisionGrid.MinimumSize = new System.Drawing.Size(250, 125);
            this.revisionGrid.Name = "revisionGrid";
            this.revisionGrid.Size = new System.Drawing.Size(1097, 466);
            this.revisionGrid.TabIndex = 0;
            this.revisionGrid.DoubleClickRevision += new System.EventHandler<GitUI.UserControls.RevisionGrid.DoubleClickRevisionEventArgs>(this.revisionGrid_DoubleClickRevision);
            this.revisionGrid.SelectionChanged += new System.EventHandler(this.revisionGrid_SelectionChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.buttonGotoCommit);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 478);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(333, 33);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 9, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Find specific commit:";
            // 
            // buttonGotoCommit
            // 
            this.buttonGotoCommit.Location = new System.Drawing.Point(147, 4);
            this.buttonGotoCommit.Margin = new System.Windows.Forms.Padding(4);
            this.buttonGotoCommit.Name = "buttonGotoCommit";
            this.buttonGotoCommit.Size = new System.Drawing.Size(162, 29);
            this.buttonGotoCommit.TabIndex = 3;
            this.buttonGotoCommit.Text = "Go to commit...";
            this.buttonGotoCommit.UseVisualStyleBackColor = true;
            this.buttonGotoCommit.Click += new System.EventHandler(this.buttonGotoCommit_Click);
            // 
            // flowLayoutPanelParents
            // 
            this.flowLayoutPanelParents.Controls.Add(this.labelParents);
            this.flowLayoutPanelParents.Controls.Add(this.linkLabelParent);
            this.flowLayoutPanelParents.Controls.Add(this.linkLabelParent2);
            this.flowLayoutPanelParents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelParents.Location = new System.Drawing.Point(344, 477);
            this.flowLayoutPanelParents.Name = "flowLayoutPanelParents";
            this.flowLayoutPanelParents.Size = new System.Drawing.Size(604, 35);
            this.flowLayoutPanelParents.TabIndex = 3;
            // 
            // labelParents
            // 
            this.labelParents.AutoSize = true;
            this.labelParents.Location = new System.Drawing.Point(4, 9);
            this.labelParents.Margin = new System.Windows.Forms.Padding(4, 9, 4, 0);
            this.labelParents.Name = "labelParents";
            this.labelParents.Size = new System.Drawing.Size(69, 17);
            this.labelParents.TabIndex = 0;
            this.labelParents.Text = "Parent(s):";
            this.labelParents.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabelParent
            // 
            this.linkLabelParent.AutoSize = true;
            this.linkLabelParent.Location = new System.Drawing.Point(81, 9);
            this.linkLabelParent.Margin = new System.Windows.Forms.Padding(4, 9, 4, 0);
            this.linkLabelParent.Name = "linkLabelParent";
            this.linkLabelParent.Size = new System.Drawing.Size(85, 17);
            this.linkLabelParent.TabIndex = 1;
            this.linkLabelParent.TabStop = true;
            this.linkLabelParent.Text = "sha parent 1";
            this.linkLabelParent.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelParent_LinkClicked);
            // 
            // linkLabelParent2
            // 
            this.linkLabelParent2.AutoSize = true;
            this.linkLabelParent2.Location = new System.Drawing.Point(174, 9);
            this.linkLabelParent2.Margin = new System.Windows.Forms.Padding(4, 9, 4, 0);
            this.linkLabelParent2.Name = "linkLabelParent2";
            this.linkLabelParent2.Size = new System.Drawing.Size(85, 17);
            this.linkLabelParent2.TabIndex = 1;
            this.linkLabelParent2.TabStop = true;
            this.linkLabelParent2.Text = "sha parent 2";
            this.linkLabelParent2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelParent_LinkClicked);
            // 
            // FormChooseCommit
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.ClientSize = new System.Drawing.Size(1105, 515);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormChooseCommit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Commit";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanelParents.ResumeLayout(false);
            this.flowLayoutPanelParents.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private RevisionGridControl revisionGrid;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonGotoCommit;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelParents;
        private System.Windows.Forms.Label labelParents;
        private System.Windows.Forms.LinkLabel linkLabelParent;
        private System.Windows.Forms.LinkLabel linkLabelParent2;
    }
}