namespace GitUI.CommandsDialogs.SettingsDialog
{
    partial class FormAvailableEncodings
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
            this.ListIncludedEncodings = new System.Windows.Forms.ListBox();
            this.ListAvailableEncodings = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ToRight = new System.Windows.Forms.Button();
            this.ToLeft = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.lSelectedEncodings = new System.Windows.Forms.Label();
            this.lAvaolableEncodings = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.ListIncludedEncodings, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.ListAvailableEncodings, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lSelectedEncodings, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lAvaolableEncodings, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(605, 315);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // ListIncludedEncodings
            // 
            this.ListIncludedEncodings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListIncludedEncodings.FormattingEnabled = true;
            this.ListIncludedEncodings.Location = new System.Drawing.Point(3, 23);
            this.ListIncludedEncodings.Name = "ListIncludedEncodings";
            this.ListIncludedEncodings.Size = new System.Drawing.Size(266, 252);
            this.ListIncludedEncodings.TabIndex = 0;
            this.ListIncludedEncodings.SelectedValueChanged += new System.EventHandler(this.ListIncludedEncodings_SelectedValueChanged);
            // 
            // ListAvailableEncodings
            // 
            this.ListAvailableEncodings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListAvailableEncodings.FormattingEnabled = true;
            this.ListAvailableEncodings.Location = new System.Drawing.Point(315, 23);
            this.ListAvailableEncodings.Name = "ListAvailableEncodings";
            this.ListAvailableEncodings.Size = new System.Drawing.Size(287, 252);
            this.ListAvailableEncodings.Sorted = true;
            this.ListAvailableEncodings.TabIndex = 1;
            this.ListAvailableEncodings.SelectedValueChanged += new System.EventHandler(this.ListAvailableEncodings_SelectedValueChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.ToRight, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.ToLeft, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(275, 23);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(34, 252);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // ToRight
            // 
            this.ToRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToRight.Enabled = false;
            this.ToRight.Location = new System.Drawing.Point(3, 99);
            this.ToRight.Name = "ToRight";
            this.ToRight.Size = new System.Drawing.Size(28, 24);
            this.ToRight.TabIndex = 0;
            this.ToRight.Text = ">";
            this.ToRight.UseVisualStyleBackColor = true;
            this.ToRight.Click += new System.EventHandler(this.ToRight_Click);
            // 
            // ToLeft
            // 
            this.ToLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToLeft.Enabled = false;
            this.ToLeft.Location = new System.Drawing.Point(3, 129);
            this.ToLeft.Name = "ToLeft";
            this.ToLeft.Size = new System.Drawing.Size(28, 24);
            this.ToLeft.TabIndex = 1;
            this.ToLeft.Text = "<";
            this.ToLeft.UseVisualStyleBackColor = true;
            this.ToLeft.Click += new System.EventHandler(this.ToLeft_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.Controls.Add(this.ButtonOk, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.ButtonCancel, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(315, 281);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(287, 31);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonOk.Location = new System.Drawing.Point(193, 3);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(91, 25);
            this.ButtonOk.TabIndex = 0;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonCancel.Location = new System.Drawing.Point(98, 3);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(89, 25);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // lSelectedEncodings
            // 
            this.lSelectedEncodings.AutoSize = true;
            this.lSelectedEncodings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lSelectedEncodings.Location = new System.Drawing.Point(3, 0);
            this.lSelectedEncodings.Name = "lSelectedEncodings";
            this.lSelectedEncodings.Size = new System.Drawing.Size(266, 20);
            this.lSelectedEncodings.TabIndex = 4;
            this.lSelectedEncodings.Text = "Selected:";
            this.lSelectedEncodings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lAvaolableEncodings
            // 
            this.lAvaolableEncodings.AutoSize = true;
            this.lAvaolableEncodings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lAvaolableEncodings.Location = new System.Drawing.Point(315, 0);
            this.lAvaolableEncodings.Name = "lAvaolableEncodings";
            this.lAvaolableEncodings.Size = new System.Drawing.Size(287, 20);
            this.lAvaolableEncodings.TabIndex = 5;
            this.lAvaolableEncodings.Text = "Available:";
            this.lAvaolableEncodings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormAvailableEncodings
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(605, 315);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAvailableEncodings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure available encodings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox ListIncludedEncodings;
        private System.Windows.Forms.ListBox ListAvailableEncodings;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button ToRight;
        private System.Windows.Forms.Button ToLeft;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label lSelectedEncodings;
        private System.Windows.Forms.Label lAvaolableEncodings;
    }
}