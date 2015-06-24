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
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.Orphan = new System.Windows.Forms.CheckBox();
            this.ClearOrphan = new System.Windows.Forms.CheckBox();
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.BranchNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.table.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // Orphan
            // 
            this.Orphan.AutoSize = true;
            this.Orphan.Location = new System.Drawing.Point(3, 3);
            this.Orphan.Name = "Orphan";
            this.Orphan.Size = new System.Drawing.Size(101, 19);
            this.Orphan.TabIndex = 7;
            this.Orphan.Text = "Create orphan";
            this.toolTip.SetToolTip(this.Orphan, "New branch will have NO parents");
            this.Orphan.UseVisualStyleBackColor = true;
            this.Orphan.CheckedChanged += new System.EventHandler(this.Orphan_CheckedChanged);
            // 
            // ClearOrphan
            // 
            this.ClearOrphan.AutoSize = true;
            this.ClearOrphan.Checked = true;
            this.ClearOrphan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ClearOrphan.Enabled = false;
            this.ClearOrphan.Location = new System.Drawing.Point(110, 3);
            this.ClearOrphan.Name = "ClearOrphan";
            this.ClearOrphan.Size = new System.Drawing.Size(170, 19);
            this.ClearOrphan.TabIndex = 8;
            this.ClearOrphan.Text = "Clear working directory and index";
            this.toolTip.SetToolTip(this.ClearOrphan, "Remove files from the working directory and from the index");
            this.ClearOrphan.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Image = global::GitUI.Properties.Resources.IconBranchCreate;
            this.Ok.Location = new System.Drawing.Point(311, 8);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(128, 25);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Create branch";
            this.Ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gotoUserManualControl1, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.table, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 216);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.commitPickerSmallControl1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(50, 30);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(442, 34);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Create branch at this revision";
            // 
            // commitPickerSmallControl1
            // 
            this.commitPickerSmallControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitPickerSmallControl1.Location = new System.Drawing.Point(169, 3);
            this.commitPickerSmallControl1.MinimumSize = new System.Drawing.Size(100, 26);
            this.commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            this.commitPickerSmallControl1.Size = new System.Drawing.Size(207, 26);
            this.commitPickerSmallControl1.TabIndex = 0;
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.AutoSize = true;
            this.gotoUserManualControl1.Location = new System.Drawing.Point(3, 189);
            this.gotoUserManualControl1.ManualSectionAnchorName = "create-branch";
            this.gotoUserManualControl1.ManualSectionSubfolder = "branches";
            this.gotoUserManualControl1.MinimumSize = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.TabIndex = 11;
            // 
            // table
            // 
            this.table.AutoSize = true;
            this.table.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.table.ColumnCount = 3;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.Controls.Add(this.Ok, 2, 0);
            this.table.Controls.Add(this.BranchNameTextBox, 1, 0);
            this.table.Controls.Add(this.label1, 0, 0);
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.Location = new System.Drawing.Point(3, 43);
            this.table.Name = "table";
            this.table.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.table.RowCount = 1;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.table.Size = new System.Drawing.Size(442, 34);
            this.table.TabIndex = 1;
            // 
            // BranchNameTextBox
            // 
            this.BranchNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BranchNameTextBox.Location = new System.Drawing.Point(86, 8);
            this.BranchNameTextBox.Name = "BranchNameTextBox";
            this.BranchNameTextBox.Size = new System.Drawing.Size(219, 23);
            this.BranchNameTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Branch name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.flowLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(3, 113);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 50);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Orphan";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.Orphan);
            this.flowLayoutPanel2.Controls.Add(this.ClearOrphan);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 19);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(432, 28);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.CheckoutAfterCreate);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 83);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(442, 24);
            this.flowLayoutPanel3.TabIndex = 2;
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(300, 3);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(139, 19);
            this.CheckoutAfterCreate.TabIndex = 6;
            this.CheckoutAfterCreate.Text = "Checkout after create";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // FormCreateBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(454, 222);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(470, 260);
            this.Name = "FormCreateBranch";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create branch";
            this.Shown += new System.EventHandler(this.FormCreateBranch_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel table;
        private System.Windows.Forms.CheckBox Orphan;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox ClearOrphan;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BranchNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
    }
}
