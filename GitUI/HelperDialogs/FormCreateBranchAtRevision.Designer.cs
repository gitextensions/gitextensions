namespace GitUI.HelperDialogs
{
    partial class FormCreateBranchAtRevision
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
            this.label1 = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.BranchNameTextBox = new System.Windows.Forms.TextBox();
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.stackOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.Orphan = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ClearOrphan = new System.Windows.Forms.CheckBox();
            this.stackOptions.SuspendLayout();
            this.table.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Branch name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(340, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(128, 25);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Create branch";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // BranchNameTextBox
            // 
            this.BranchNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BranchNameTextBox.Location = new System.Drawing.Point(92, 3);
            this.BranchNameTextBox.Name = "BranchNameTextBox";
            this.BranchNameTextBox.Size = new System.Drawing.Size(242, 25);
            this.BranchNameTextBox.TabIndex = 3;
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(3, 3);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(151, 21);
            this.CheckoutAfterCreate.TabIndex = 6;
            this.CheckoutAfterCreate.Text = "Checkout after create";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // stackOptions
            // 
            this.stackOptions.AutoSize = true;
            this.stackOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.stackOptions.Controls.Add(this.CheckoutAfterCreate);
            this.stackOptions.Controls.Add(this.Orphan);
            this.stackOptions.Controls.Add(this.ClearOrphan);
            this.stackOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.stackOptions.Location = new System.Drawing.Point(0, 31);
            this.stackOptions.Name = "stackOptions";
            this.stackOptions.Size = new System.Drawing.Size(471, 27);
            this.stackOptions.TabIndex = 8;
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
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Name = "table";
            this.table.RowCount = 1;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.table.Size = new System.Drawing.Size(471, 31);
            this.table.TabIndex = 6;
            // 
            // Orphan
            // 
            this.Orphan.AutoSize = true;
            this.Orphan.Location = new System.Drawing.Point(160, 3);
            this.Orphan.Name = "Orphan";
            this.Orphan.Size = new System.Drawing.Size(71, 21);
            this.Orphan.TabIndex = 7;
            this.Orphan.Text = "Orphan";
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
            this.ClearOrphan.Location = new System.Drawing.Point(237, 3);
            this.ClearOrphan.Name = "ClearOrphan";
            this.ClearOrphan.Size = new System.Drawing.Size(57, 21);
            this.ClearOrphan.TabIndex = 8;
            this.ClearOrphan.Text = "Clear";
            this.toolTip.SetToolTip(this.ClearOrphan, "Remove files from the working tree and from the index");
            this.ClearOrphan.UseVisualStyleBackColor = true;
            // 
            // FormBranchSmall
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(471, 72);
            this.Controls.Add(this.stackOptions);
            this.Controls.Add(this.table);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBranchSmall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create branch";
            this.stackOptions.ResumeLayout(false);
            this.stackOptions.PerformLayout();
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BranchNameTextBox;
        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
        private System.Windows.Forms.FlowLayoutPanel stackOptions;
        private System.Windows.Forms.TableLayoutPanel table;
        private System.Windows.Forms.CheckBox Orphan;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox ClearOrphan;
    }
}