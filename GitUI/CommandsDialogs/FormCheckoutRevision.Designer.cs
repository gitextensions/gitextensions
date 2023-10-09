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
            commitPickerSmallControl1 = new UserControls.CommitPickerSmallControl();
            label2 = new Label();
            Force = new CheckBox();
            OkCheckout = new Button();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tableLayoutPanel1);
            MainPanel.Size = new Size(481, 90);
            MainPanel.TabIndex = 0;
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(OkCheckout);
            ControlsPanel.Location = new Point(0, 90);
            ControlsPanel.Size = new Size(481, 41);
            ControlsPanel.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(commitPickerSmallControl1, 1, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 0);
            tableLayoutPanel1.Controls.Add(Force, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(457, 66);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // commitPickerSmallControl1
            // 
            commitPickerSmallControl1.AutoSize = true;
            commitPickerSmallControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitPickerSmallControl1.Dock = DockStyle.Fill;
            commitPickerSmallControl1.Location = new Point(133, 3);
            commitPickerSmallControl1.MinimumSize = new Size(100, 26);
            commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            commitPickerSmallControl1.Size = new Size(321, 26);
            commitPickerSmallControl1.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 5);
            label2.Margin = new Padding(3, 5, 3, 0);
            label2.Name = "label2";
            label2.Size = new Size(124, 27);
            label2.TabIndex = 0;
            label2.Text = "Checkout this &revision";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Force
            // 
            Force.Anchor = AnchorStyles.Left;
            Force.AutoSize = true;
            Force.Location = new Point(133, 35);
            Force.Name = "Force";
            Force.Size = new Size(166, 19);
            Force.TabIndex = 5;
            Force.Text = "&Force (reset local changes)";
            Force.UseVisualStyleBackColor = true;
            // 
            // OkCheckout
            // 
            OkCheckout.Anchor = AnchorStyles.Right;
            OkCheckout.AutoSize = true;
            OkCheckout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OkCheckout.Image = Properties.Images.Checkout;
            OkCheckout.Location = new Point(384, 8);
            OkCheckout.MinimumSize = new Size(75, 23);
            OkCheckout.Name = "OkCheckout";
            OkCheckout.Size = new Size(84, 25);
            OkCheckout.TabIndex = 0;
            OkCheckout.Text = "&Checkout";
            OkCheckout.TextAlign = ContentAlignment.MiddleRight;
            OkCheckout.TextImageRelation = TextImageRelation.ImageBeforeText;
            OkCheckout.UseVisualStyleBackColor = true;
            OkCheckout.Click += OkClick;
            // 
            // FormCheckoutRevision
            // 
            AcceptButton = OkCheckout;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(481, 131);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(460, 125);
            Name = "FormCheckoutRevision";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Checkout revision";
            MainPanel.ResumeLayout(false);
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button OkCheckout;
        private System.Windows.Forms.CheckBox Force;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
    }
}
