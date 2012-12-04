namespace GitUI
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
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.Checkout = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.BName = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(3, 3);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.RevisionGraphDrawStyle = GitUI.DvcsGraph.RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
            this.RevisionGrid.Size = new System.Drawing.Size(740, 437);
            this.RevisionGrid.TabIndex = 0;
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(262, 6);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(139, 19);
            this.CheckoutAfterCreate.TabIndex = 4;
            this.CheckoutAfterCreate.Text = "Checkout after create";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // Checkout
            // 
            this.Checkout.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Checkout.Location = new System.Drawing.Point(407, 3);
            this.Checkout.Name = "Checkout";
            this.Checkout.Size = new System.Drawing.Size(160, 25);
            this.Checkout.TabIndex = 3;
            this.Checkout.Text = "Checkout branch";
            this.Checkout.UseVisualStyleBackColor = true;
            this.Checkout.Click += new System.EventHandler(this.Checkout_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Branch name";
            // 
            // Ok
            // 
            this.Ok.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Ok.Location = new System.Drawing.Point(573, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(160, 25);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Create branch";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // BName
            // 
            this.BName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BName.Location = new System.Drawing.Point(86, 3);
            this.BName.Name = "BName";
            this.BName.Size = new System.Drawing.Size(170, 23);
            this.BName.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.RevisionGrid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(746, 484);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.BName, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.CheckoutAfterCreate, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Checkout, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Ok, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 448);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(736, 31);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // FormBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(746, 484);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormBranch";
            this.Text = "Create Branch";
            this.Load += new System.EventHandler(this.FormBranch_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BName;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.Button Checkout;
        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}