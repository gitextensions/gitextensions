﻿namespace GitUI
{
    partial class FormBranch
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.Checkout = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.BName = new System.Windows.Forms.TextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RevisionGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.CheckoutAfterCreate);
            this.splitContainer1.Panel2.Controls.Add(this.Checkout);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Panel2.Controls.Add(this.BName);
            this.splitContainer1.Size = new System.Drawing.Size(746, 484);
            this.splitContainer1.SplitterDistance = 445;
            this.splitContainer1.TabIndex = 0;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.AllowGraphWithFilter = false;
            this.RevisionGrid.BranchFilter = "";
            this.RevisionGrid.CurrentCheckout = "";
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.RevisionGrid.InMemAuthorFilter = "";
            this.RevisionGrid.InMemCommitterFilter = "";
            this.RevisionGrid.InMemFilterIgnoreCase = false;
            this.RevisionGrid.InMemMessageFilter = "";
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.MultiSelect = false;
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RevisionGrid.Size = new System.Drawing.Size(746, 445);
            this.RevisionGrid.TabIndex = 0;
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(244, 9);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(132, 17);
            this.CheckoutAfterCreate.TabIndex = 4;
            this.CheckoutAfterCreate.Text = "Checkout after create";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // Checkout
            // 
            this.Checkout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Checkout.Location = new System.Drawing.Point(414, 6);
            this.Checkout.Name = "Checkout";
            this.Checkout.Size = new System.Drawing.Size(160, 25);
            this.Checkout.TabIndex = 3;
            this.Checkout.Text = "Checkout branch";
            this.Checkout.UseVisualStyleBackColor = true;
            this.Checkout.Click += new System.EventHandler(this.Checkout_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Branch name";
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(580, 5);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(160, 25);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Create branch";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // BName
            // 
            this.BName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BName.Location = new System.Drawing.Point(112, 6);
            this.BName.Name = "BName";
            this.BName.Size = new System.Drawing.Size(126, 21);
            this.BName.TabIndex = 0;
            // 
            // FormBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 484);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormBranch";
            this.Text = "Create Branch";
            this.Load += new System.EventHandler(this.FormBranch_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBranch_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BName;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.Button Checkout;
        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
    }
}