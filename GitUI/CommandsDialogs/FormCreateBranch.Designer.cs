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
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.stackOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.BranchNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.stackOptions.SuspendLayout();
            this.table.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
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
            this.ClearOrphan.Text = "Clear working dir and index";
            this.toolTip.SetToolTip(this.ClearOrphan, "Remove files from the working tree and from the index");
            this.ClearOrphan.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Image = global::GitUI.Properties.Resources.IconBranchCreate;
            this.Ok.Location = new System.Drawing.Point(307, 8);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(128, 25);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Create branch";
            this.Ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.Location = new System.Drawing.Point(6, 181);
            this.gotoUserManualControl1.ManualSectionAnchorName = "create-branch";
            this.gotoUserManualControl1.ManualSectionSubfolder = "branches";
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(60, 18);
            this.gotoUserManualControl1.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.flowLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 119);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 54);
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
            this.flowLayoutPanel2.Size = new System.Drawing.Size(432, 32);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // stackOptions
            // 
            this.stackOptions.AutoSize = true;
            this.stackOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.stackOptions.Controls.Add(this.CheckoutAfterCreate);
            this.stackOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.stackOptions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.stackOptions.Location = new System.Drawing.Point(3, 84);
            this.stackOptions.Name = "stackOptions";
            this.stackOptions.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.stackOptions.Size = new System.Drawing.Size(438, 35);
            this.stackOptions.TabIndex = 8;
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(296, 8);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(139, 19);
            this.CheckoutAfterCreate.TabIndex = 6;
            this.CheckoutAfterCreate.Text = "Checkout after create";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
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
            this.table.Dock = System.Windows.Forms.DockStyle.Top;
            this.table.Location = new System.Drawing.Point(3, 43);
            this.table.Name = "table";
            this.table.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.table.RowCount = 1;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.table.Size = new System.Drawing.Size(438, 41);
            this.table.TabIndex = 1;
            // 
            // BranchNameTextBox
            // 
            this.BranchNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BranchNameTextBox.Location = new System.Drawing.Point(86, 8);
            this.BranchNameTextBox.Name = "BranchNameTextBox";
            this.BranchNameTextBox.Size = new System.Drawing.Size(215, 23);
            this.BranchNameTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Branch name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.panel1.Size = new System.Drawing.Size(438, 40);
            this.panel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.commitPickerSmallControl1);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 5);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(50, 30);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(438, 30);
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
            this.commitPickerSmallControl1.Size = new System.Drawing.Size(129, 26);
            this.commitPickerSmallControl1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(304, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "or choose another one.";
            // 
            // FormCreateBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(444, 202);
            this.Controls.Add(this.gotoUserManualControl1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.stackOptions);
            this.Controls.Add(this.table);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(460, 240);
            this.Name = "FormCreateBranch";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create branch";
            this.Load += new System.EventHandler(this.FormCreateBranch_Load);
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.stackOptions.ResumeLayout(false);
            this.stackOptions.PerformLayout();
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
        private System.Windows.Forms.FlowLayoutPanel stackOptions;
        private System.Windows.Forms.TableLayoutPanel table;
        private System.Windows.Forms.CheckBox Orphan;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox ClearOrphan;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BranchNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
    }
}