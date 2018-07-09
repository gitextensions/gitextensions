namespace GitUI.CommandsDialogs
{
    partial class FormCheckoutRevision
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.OkCheckout = new System.Windows.Forms.Button();
            this.Force = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(444, 106);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 69.71545F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.28455F));
            this.tableLayoutPanel2.Controls.Add(this.OkCheckout, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.Force, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 70);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(438, 31);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // OkCheckout
            // 
            this.OkCheckout.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.OkCheckout.Image = global::GitUI.Properties.Images.Checkout;
            this.OkCheckout.Location = new System.Drawing.Point(313, 3);
            this.OkCheckout.Name = "OkCheckout";
            this.OkCheckout.Size = new System.Drawing.Size(122, 25);
            this.OkCheckout.TabIndex = 1;
            this.OkCheckout.Text = "Checkout";
            this.OkCheckout.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.OkCheckout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.OkCheckout.UseVisualStyleBackColor = true;
            this.OkCheckout.Click += new System.EventHandler(this.OkClick);
            // 
            // Force
            // 
            this.Force.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Force.AutoSize = true;
            this.Force.Location = new System.Drawing.Point(3, 6);
            this.Force.Name = "Force";
            this.Force.Size = new System.Drawing.Size(166, 19);
            this.Force.TabIndex = 5;
            this.Force.Text = "Force (reset local changes)";
            this.Force.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.commitPickerSmallControl1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(438, 61);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Checkout this revision";
            // 
            // commitPickerSmallControl1
            // 
            this.commitPickerSmallControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitPickerSmallControl1.Location = new System.Drawing.Point(133, 3);
            this.commitPickerSmallControl1.MinimumSize = new System.Drawing.Size(100, 26);
            this.commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            this.commitPickerSmallControl1.Size = new System.Drawing.Size(207, 26);
            this.commitPickerSmallControl1.TabIndex = 3;
            // 
            // FormCheckoutRevision
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(444, 106);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(460, 125);
            this.Name = "FormCheckoutRevision";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout revision";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OkCheckout;
        private System.Windows.Forms.CheckBox Force;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
    }
}
