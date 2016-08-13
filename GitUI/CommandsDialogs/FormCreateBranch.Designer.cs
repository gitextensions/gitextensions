namespace GitUI.CommandsDialogs
{
    partial class FormCreateBranch
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
            this.label2 = new System.Windows.Forms.Label();
            this.commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            this.chkbxCheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BranchNameTextBox = new System.Windows.Forms.TextBox();
            this.chkbxOrphan = new System.Windows.Forms.CheckBox();
            this.chkbxClearOrphan = new System.Windows.Forms.CheckBox();
            this.Ok = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.CheckoutAfterCreate = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ClearOrphan = new System.Windows.Forms.Label();
            this.Orphan = new System.Windows.Forms.Label();
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 31);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 22);
            this.label2.TabIndex = 2;
            this.label2.Text = "Create b&ranch at this revision";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // commitPickerSmallControl1
            // 
            this.commitPickerSmallControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitPickerSmallControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitPickerSmallControl1.Location = new System.Drawing.Point(223, 31);
            this.commitPickerSmallControl1.MinimumSize = new System.Drawing.Size(100, 26);
            this.commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            this.commitPickerSmallControl1.Size = new System.Drawing.Size(242, 26);
            this.commitPickerSmallControl1.TabIndex = 3;
            // 
            // chkbxCheckoutAfterCreate
            // 
            this.chkbxCheckoutAfterCreate.AutoSize = true;
            this.chkbxCheckoutAfterCreate.Checked = true;
            this.chkbxCheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxCheckoutAfterCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkbxCheckoutAfterCreate.Location = new System.Drawing.Point(223, 59);
            this.chkbxCheckoutAfterCreate.Name = "chkbxCheckoutAfterCreate";
            this.chkbxCheckoutAfterCreate.Size = new System.Drawing.Size(242, 22);
            this.chkbxCheckoutAfterCreate.TabIndex = 5;
            this.chkbxCheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Branch name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BranchNameTextBox
            // 
            this.BranchNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchNameTextBox.Location = new System.Drawing.Point(223, 3);
            this.BranchNameTextBox.Name = "BranchNameTextBox";
            this.BranchNameTextBox.Size = new System.Drawing.Size(242, 21);
            this.BranchNameTextBox.TabIndex = 1;
            this.BranchNameTextBox.TextChanged += new System.EventHandler(this.BranchNameTextBox_TextChanged);
            // 
            // chkbxOrphan
            // 
            this.chkbxOrphan.AutoSize = true;
            this.chkbxOrphan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkbxOrphan.Location = new System.Drawing.Point(217, 3);
            this.chkbxOrphan.Name = "chkbxOrphan";
            this.chkbxOrphan.Size = new System.Drawing.Size(236, 22);
            this.chkbxOrphan.TabIndex = 1;
            this.toolTip.SetToolTip(this.chkbxOrphan, "New branch will have NO parents");
            this.chkbxOrphan.UseVisualStyleBackColor = true;
            this.chkbxOrphan.CheckedChanged += new System.EventHandler(this.Orphan_CheckedChanged);
            // 
            // chkbxClearOrphan
            // 
            this.chkbxClearOrphan.AutoSize = true;
            this.chkbxClearOrphan.Checked = true;
            this.chkbxClearOrphan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxClearOrphan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkbxClearOrphan.Enabled = false;
            this.chkbxClearOrphan.Location = new System.Drawing.Point(217, 31);
            this.chkbxClearOrphan.Name = "chkbxClearOrphan";
            this.chkbxClearOrphan.Size = new System.Drawing.Size(236, 22);
            this.chkbxClearOrphan.TabIndex = 3;
            this.toolTip.SetToolTip(this.chkbxClearOrphan, "Remove files from the working directory and from the index");
            this.chkbxClearOrphan.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Image = global::GitUI.Properties.Resources.IconBranchCreate;
            this.Ok.Location = new System.Drawing.Point(337, 169);
            this.Ok.MinimumSize = new System.Drawing.Size(0, 30);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(128, 30);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "&Create branch";
            this.Ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.commitPickerSmallControl1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.BranchNameTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.CheckoutAfterCreate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkbxCheckoutAfterCreate, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.Ok, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.gotoUserManualControl1, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(468, 195);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(3, 59);
            this.CheckoutAfterCreate.Margin = new System.Windows.Forms.Padding(3);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(214, 22);
            this.CheckoutAfterCreate.TabIndex = 4;
            this.CheckoutAfterCreate.Text = "Checkout &after create";
            this.CheckoutAfterCreate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 87);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(462, 76);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Orphan";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.ClearOrphan, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.chkbxClearOrphan, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.chkbxOrphan, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.Orphan, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(456, 56);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // ClearOrphan
            // 
            this.ClearOrphan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClearOrphan.Location = new System.Drawing.Point(3, 31);
            this.ClearOrphan.Margin = new System.Windows.Forms.Padding(3);
            this.ClearOrphan.Name = "ClearOrphan";
            this.ClearOrphan.Size = new System.Drawing.Size(208, 22);
            this.ClearOrphan.TabIndex = 2;
            this.ClearOrphan.Text = "Clear &working directory and index";
            this.ClearOrphan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Orphan
            // 
            this.Orphan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Orphan.Location = new System.Drawing.Point(3, 3);
            this.Orphan.Margin = new System.Windows.Forms.Padding(3);
            this.Orphan.Name = "Orphan";
            this.Orphan.Size = new System.Drawing.Size(208, 22);
            this.Orphan.TabIndex = 0;
            this.Orphan.Text = "Create or&phan";
            this.Orphan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.AutoSize = true;
            this.gotoUserManualControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gotoUserManualControl1.Location = new System.Drawing.Point(3, 174);
            this.gotoUserManualControl1.ManualSectionAnchorName = "create-branch";
            this.gotoUserManualControl1.ManualSectionSubfolder = "branches";
            this.gotoUserManualControl1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.gotoUserManualControl1.MinimumSize = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.TabIndex = 8;
            // 
            // FormCreateBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(484, 211);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 250);
            this.Name = "FormCreateBranch";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create branch";
            this.Shown += new System.EventHandler(this.FormCreateBranch_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxOrphan;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox chkbxClearOrphan;
        private System.Windows.Forms.TextBox BranchNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.CheckBox chkbxCheckoutAfterCreate;
        private System.Windows.Forms.Label CheckoutAfterCreate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label ClearOrphan;
        private System.Windows.Forms.Label Orphan;
    }
}
