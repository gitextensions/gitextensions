namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class AdvancedSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.CheckoutGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAlwaysShowCheckoutDlg = new System.Windows.Forms.CheckBox();
            this.chkUseLocalChangesAction = new System.Windows.Forms.CheckBox();
            this.GeneralGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAlwaysShowAdvOpt = new System.Windows.Forms.CheckBox();
            this.chkDontSHowHelpImages = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.CheckoutGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.GeneralGB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.CheckoutGB, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.GeneralGB, 0, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 12);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(457, 220);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // CheckoutGB
            // 
            this.CheckoutGB.AutoSize = true;
            this.CheckoutGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CheckoutGB.Controls.Add(this.tableLayoutPanel3);
            this.CheckoutGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckoutGB.Location = new System.Drawing.Point(3, 3);
            this.CheckoutGB.Name = "CheckoutGB";
            this.CheckoutGB.Size = new System.Drawing.Size(451, 104);
            this.CheckoutGB.TabIndex = 0;
            this.CheckoutGB.TabStop = false;
            this.CheckoutGB.Text = "Checkout";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.chkAlwaysShowCheckoutDlg, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.chkUseLocalChangesAction, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(9, 21);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(436, 62);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkAlwaysShowCheckoutDlg
            // 
            this.chkAlwaysShowCheckoutDlg.AutoSize = true;
            this.chkAlwaysShowCheckoutDlg.Location = new System.Drawing.Point(3, 3);
            this.chkAlwaysShowCheckoutDlg.Name = "chkAlwaysShowCheckoutDlg";
            this.chkAlwaysShowCheckoutDlg.Size = new System.Drawing.Size(186, 18);
            this.chkAlwaysShowCheckoutDlg.TabIndex = 0;
            this.chkAlwaysShowCheckoutDlg.Text = "Always show checkout dialog";
            this.chkAlwaysShowCheckoutDlg.UseVisualStyleBackColor = true;
            // 
            // chkUseLocalChangesAction
            // 
            this.chkUseLocalChangesAction.AutoSize = true;
            this.chkUseLocalChangesAction.Location = new System.Drawing.Point(3, 27);
            this.chkUseLocalChangesAction.Name = "chkUseLocalChangesAction";
            this.chkUseLocalChangesAction.Size = new System.Drawing.Size(430, 32);
            this.chkUseLocalChangesAction.TabIndex = 1;
            this.chkUseLocalChangesAction.Text = "Use last chosen \"local changes\" action as default action.\r\nThis action will be pe" +
    "rformed without warning while checking out branch.";
            this.chkUseLocalChangesAction.UseVisualStyleBackColor = true;
            // 
            // GeneralGB
            // 
            this.GeneralGB.AutoSize = true;
            this.GeneralGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GeneralGB.Controls.Add(this.tableLayoutPanel1);
            this.GeneralGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.GeneralGB.Location = new System.Drawing.Point(3, 113);
            this.GeneralGB.Name = "GeneralGB";
            this.GeneralGB.Size = new System.Drawing.Size(451, 66);
            this.GeneralGB.TabIndex = 1;
            this.GeneralGB.TabStop = false;
            this.GeneralGB.Text = "General";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chkAlwaysShowAdvOpt, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkDontSHowHelpImages, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 21);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(163, 24);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chkAlwaysShowAdvOpt
            // 
            this.chkAlwaysShowAdvOpt.AutoSize = true;
            this.chkAlwaysShowAdvOpt.Location = new System.Drawing.Point(3, 32);
            this.chkAlwaysShowAdvOpt.Name = "chkAlwaysShowAdvOpt";
            this.chkAlwaysShowAdvOpt.Size = new System.Drawing.Size(208, 21);
            this.chkAlwaysShowAdvOpt.TabIndex = 2;
            this.chkAlwaysShowAdvOpt.Text = "Always show advanced options";
            this.chkAlwaysShowAdvOpt.UseVisualStyleBackColor = true;
            // 
            // chkDontSHowHelpImages
            // 
            this.chkDontSHowHelpImages.AutoSize = true;
            this.chkDontSHowHelpImages.Location = new System.Drawing.Point(3, 3);
            this.chkDontSHowHelpImages.Name = "chkDontSHowHelpImages";
            this.chkDontSHowHelpImages.Size = new System.Drawing.Size(157, 18);
            this.chkDontSHowHelpImages.TabIndex = 1;
            this.chkDontSHowHelpImages.Text = "Don\'t show help images";
            this.chkDontSHowHelpImages.UseVisualStyleBackColor = true;
            // 
            // AdvancedSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "AdvancedSettingsPage";
            this.Size = new System.Drawing.Size(701, 459);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.CheckoutGB.ResumeLayout(false);
            this.CheckoutGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.GeneralGB.ResumeLayout(false);
            this.GeneralGB.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox CheckoutGB;
        private System.Windows.Forms.GroupBox GeneralGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkDontSHowHelpImages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox chkAlwaysShowCheckoutDlg;
        private System.Windows.Forms.CheckBox chkUseLocalChangesAction;
        private System.Windows.Forms.CheckBox chkAlwaysShowAdvOpt;

    }
}
