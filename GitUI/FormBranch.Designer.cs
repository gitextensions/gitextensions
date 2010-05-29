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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBranch));
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
            this.splitContainer1.AccessibleDescription = null;
            this.splitContainer1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BackgroundImage = null;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Font = null;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AccessibleDescription = null;
            this.splitContainer1.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.BackgroundImage = null;
            this.splitContainer1.Panel1.Controls.Add(this.RevisionGrid);
            this.splitContainer1.Panel1.Font = null;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AccessibleDescription = null;
            this.splitContainer1.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.BackgroundImage = null;
            this.splitContainer1.Panel2.Controls.Add(this.CheckoutAfterCreate);
            this.splitContainer1.Panel2.Controls.Add(this.Checkout);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Panel2.Controls.Add(this.BName);
            this.splitContainer1.Panel2.Font = null;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.AccessibleDescription = null;
            this.RevisionGrid.AccessibleName = null;
            resources.ApplyResources(this.RevisionGrid, "RevisionGrid");
            this.RevisionGrid.BackgroundImage = null;
            this.RevisionGrid.currentCheckout = null;
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.Font = null;
            this.RevisionGrid.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.AccessibleDescription = null;
            this.CheckoutAfterCreate.AccessibleName = null;
            resources.ApplyResources(this.CheckoutAfterCreate, "CheckoutAfterCreate");
            this.CheckoutAfterCreate.BackgroundImage = null;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Font = null;
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // Checkout
            // 
            this.Checkout.AccessibleDescription = null;
            this.Checkout.AccessibleName = null;
            resources.ApplyResources(this.Checkout, "Checkout");
            this.Checkout.BackgroundImage = null;
            this.Checkout.Font = null;
            this.Checkout.Name = "Checkout";
            this.Checkout.UseVisualStyleBackColor = true;
            this.Checkout.Click += new System.EventHandler(this.Checkout_Click);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // Ok
            // 
            this.Ok.AccessibleDescription = null;
            this.Ok.AccessibleName = null;
            resources.ApplyResources(this.Ok, "Ok");
            this.Ok.BackgroundImage = null;
            this.Ok.Font = null;
            this.Ok.Name = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // BName
            // 
            this.BName.AccessibleDescription = null;
            this.BName.AccessibleName = null;
            resources.ApplyResources(this.BName, "BName");
            this.BName.BackgroundImage = null;
            this.BName.Font = null;
            this.BName.Name = "BName";
            // 
            // FormBranch
            // 
            this.AcceptButton = this.Ok;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.splitContainer1);
            this.Font = null;
            this.Icon = null;
            this.Name = "FormBranch";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBranch_FormClosing);
            this.Load += new System.EventHandler(this.FormBranch_Load);
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