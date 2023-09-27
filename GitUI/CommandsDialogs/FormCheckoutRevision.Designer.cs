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
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            OkCheckout = new Button();
            Force = new CheckBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label2 = new Label();
            commitPickerSmallControl1 = new UserControls.CommitPickerSmallControl();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(444, 106);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69.71545F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30.28455F));
            tableLayoutPanel2.Controls.Add(OkCheckout, 1, 0);
            tableLayoutPanel2.Controls.Add(Force, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 72);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(438, 31);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // OkCheckout
            // 
            OkCheckout.Anchor = AnchorStyles.Right;
            OkCheckout.Image = Properties.Images.Checkout;
            OkCheckout.Location = new Point(313, 3);
            OkCheckout.Name = "OkCheckout";
            OkCheckout.Size = new Size(122, 25);
            OkCheckout.TabIndex = 1;
            OkCheckout.Text = "&Checkout";
            OkCheckout.TextAlign = ContentAlignment.MiddleRight;
            OkCheckout.TextImageRelation = TextImageRelation.ImageBeforeText;
            OkCheckout.UseVisualStyleBackColor = true;
            OkCheckout.Click += OkClick;
            // 
            // Force
            // 
            Force.Anchor = AnchorStyles.Left;
            Force.AutoSize = true;
            Force.Location = new Point(3, 6);
            Force.Name = "Force";
            Force.Size = new Size(166, 19);
            Force.TabIndex = 5;
            Force.Text = "&Force (reset local changes)";
            Force.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(commitPickerSmallControl1);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(438, 63);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 5);
            label2.Margin = new Padding(3, 5, 3, 0);
            label2.Name = "label2";
            label2.Size = new Size(124, 15);
            label2.TabIndex = 2;
            label2.Text = "Checkout this &revision";
            // 
            // commitPickerSmallControl1
            // 
            commitPickerSmallControl1.AutoSize = true;
            commitPickerSmallControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitPickerSmallControl1.Location = new Point(133, 3);
            commitPickerSmallControl1.MinimumSize = new Size(100, 26);
            commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            commitPickerSmallControl1.Size = new Size(156, 26);
            commitPickerSmallControl1.TabIndex = 3;
            // 
            // FormCheckoutRevision
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(444, 106);
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(460, 125);
            Name = "FormCheckoutRevision";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Checkout revision";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
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
