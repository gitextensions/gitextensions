namespace GitUI.SettingsDialog.Pages
{
    partial class ConfirmationsSettingsPage
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
            this.chkAmmend = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterPull = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.CheckoutGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.CheckoutGB, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(559, 96);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // CheckoutGB
            // 
            this.CheckoutGB.AutoSize = true;
            this.CheckoutGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CheckoutGB.Controls.Add(this.tableLayoutPanel3);
            this.CheckoutGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckoutGB.Location = new System.Drawing.Point(3, 3);
            this.CheckoutGB.Name = "CheckoutGB";
            this.CheckoutGB.Size = new System.Drawing.Size(553, 90);
            this.CheckoutGB.TabIndex = 0;
            this.CheckoutGB.TabStop = false;
            this.CheckoutGB.Text = "Don\'t ask to confirm to (use with caution)";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.chkAmmend, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.chkAutoPopStashAfterPull, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(9, 21);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(538, 48);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkAmmend
            // 
            this.chkAmmend.AutoSize = true;
            this.chkAmmend.Location = new System.Drawing.Point(3, 3);
            this.chkAmmend.Name = "chkAmmend";
            this.chkAmmend.Size = new System.Drawing.Size(141, 18);
            this.chkAmmend.TabIndex = 0;
            this.chkAmmend.Text = "Ammend last commit";
            this.chkAmmend.UseVisualStyleBackColor = true;
            // 
            // chkUseLocalChangesAction
            // 
            this.chkAutoPopStashAfterPull.AutoSize = true;
            this.chkAutoPopStashAfterPull.Location = new System.Drawing.Point(3, 27);
            this.chkAutoPopStashAfterPull.Name = "chkUseLocalChangesAction";
            this.chkAutoPopStashAfterPull.Size = new System.Drawing.Size(532, 18);
            this.chkAutoPopStashAfterPull.TabIndex = 1;
            this.chkAutoPopStashAfterPull.Text = "Apply automatically stashed changes.  Stash will be automatically poped after suc" +
                "cessful pull.";
            this.chkAutoPopStashAfterPull.UseVisualStyleBackColor = true;
            // 
            // ConfirmationsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "ConfirmationsSettingsPage";
            this.Size = new System.Drawing.Size(649, 384);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.CheckoutGB.ResumeLayout(false);
            this.CheckoutGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox CheckoutGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox chkAmmend;
        private System.Windows.Forms.CheckBox chkAutoPopStashAfterPull;

    }
}
