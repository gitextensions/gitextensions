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
            ListIncludedEncodings = new ListBox();
            ListAvailableEncodings = new ListBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            ToRight = new Button();
            ToLeft = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            ButtonOk = new Button();
            ButtonCancel = new Button();
            lSelectedEncodings = new Label();
            lAvaolableEncodings = new Label();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(ListIncludedEncodings, 0, 1);
            tableLayoutPanel1.Controls.Add(ListAvailableEncodings, 2, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 2, 2);
            tableLayoutPanel1.Controls.Add(lSelectedEncodings, 0, 0);
            tableLayoutPanel1.Controls.Add(lAvaolableEncodings, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tableLayoutPanel1.Size = new Size(605, 315);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // ListIncludedEncodings
            // 
            ListIncludedEncodings.Dock = DockStyle.Fill;
            ListIncludedEncodings.FormattingEnabled = true;
            ListIncludedEncodings.Location = new Point(3, 23);
            ListIncludedEncodings.Name = "ListIncludedEncodings";
            ListIncludedEncodings.Size = new Size(266, 252);
            ListIncludedEncodings.TabIndex = 0;
            ListIncludedEncodings.SelectedValueChanged += ListIncludedEncodings_SelectedValueChanged;
            // 
            // ListAvailableEncodings
            // 
            ListAvailableEncodings.Dock = DockStyle.Fill;
            ListAvailableEncodings.FormattingEnabled = true;
            ListAvailableEncodings.Location = new Point(315, 23);
            ListAvailableEncodings.Name = "ListAvailableEncodings";
            ListAvailableEncodings.Size = new Size(287, 252);
            ListAvailableEncodings.Sorted = true;
            ListAvailableEncodings.TabIndex = 1;
            ListAvailableEncodings.SelectedValueChanged += ListAvailableEncodings_SelectedValueChanged;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(ToRight, 0, 1);
            tableLayoutPanel2.Controls.Add(ToLeft, 0, 2);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(275, 23);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(34, 252);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // ToRight
            // 
            ToRight.Dock = DockStyle.Fill;
            ToRight.Enabled = false;
            ToRight.Location = new Point(3, 99);
            ToRight.Name = "ToRight";
            ToRight.Size = new Size(28, 24);
            ToRight.TabIndex = 0;
            ToRight.Text = ">";
            ToRight.UseVisualStyleBackColor = true;
            ToRight.Click += ToRight_Click;
            // 
            // ToLeft
            // 
            ToLeft.Dock = DockStyle.Fill;
            ToLeft.Enabled = false;
            ToLeft.Location = new Point(3, 129);
            ToLeft.Name = "ToLeft";
            ToLeft.Size = new Size(28, 24);
            ToLeft.TabIndex = 1;
            ToLeft.Text = "<";
            ToLeft.UseVisualStyleBackColor = true;
            ToLeft.Click += ToLeft_Click;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel3.Controls.Add(ButtonOk, 2, 0);
            tableLayoutPanel3.Controls.Add(ButtonCancel, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(315, 281);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(287, 31);
            tableLayoutPanel3.TabIndex = 3;
            // 
            // ButtonOk
            // 
            ButtonOk.Dock = DockStyle.Fill;
            ButtonOk.Location = new Point(193, 3);
            ButtonOk.Name = "ButtonOk";
            ButtonOk.Size = new Size(91, 25);
            ButtonOk.TabIndex = 0;
            ButtonOk.Text = "OK";
            ButtonOk.UseVisualStyleBackColor = true;
            ButtonOk.Click += ButtonOk_Click;
            // 
            // ButtonCancel
            // 
            ButtonCancel.DialogResult = DialogResult.Cancel;
            ButtonCancel.Dock = DockStyle.Fill;
            ButtonCancel.Location = new Point(98, 3);
            ButtonCancel.Name = "ButtonCancel";
            ButtonCancel.Size = new Size(89, 25);
            ButtonCancel.TabIndex = 1;
            ButtonCancel.Text = "Cancel";
            ButtonCancel.UseVisualStyleBackColor = true;
            ButtonCancel.Click += ButtonCancel_Click;
            // 
            // lSelectedEncodings
            // 
            lSelectedEncodings.AutoSize = true;
            lSelectedEncodings.Dock = DockStyle.Fill;
            lSelectedEncodings.Location = new Point(3, 0);
            lSelectedEncodings.Name = "lSelectedEncodings";
            lSelectedEncodings.Size = new Size(266, 20);
            lSelectedEncodings.TabIndex = 4;
            lSelectedEncodings.Text = "Selected:";
            lSelectedEncodings.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lAvaolableEncodings
            // 
            lAvaolableEncodings.AutoSize = true;
            lAvaolableEncodings.Dock = DockStyle.Fill;
            lAvaolableEncodings.Location = new Point(315, 0);
            lAvaolableEncodings.Name = "lAvaolableEncodings";
            lAvaolableEncodings.Size = new Size(287, 20);
            lAvaolableEncodings.TabIndex = 5;
            lAvaolableEncodings.Text = "Available:";
            lAvaolableEncodings.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FormAvailableEncodings
            // 
            AcceptButton = ButtonOk;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = ButtonCancel;
            ClientSize = new Size(605, 315);
            Controls.Add(tableLayoutPanel1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAvailableEncodings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configure available encodings";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private ListBox ListIncludedEncodings;
        private ListBox ListAvailableEncodings;
        private TableLayoutPanel tableLayoutPanel2;
        private Button ToRight;
        private Button ToLeft;
        private TableLayoutPanel tableLayoutPanel3;
        private Button ButtonOk;
        private Button ButtonCancel;
        private Label lSelectedEncodings;
        private Label lAvaolableEncodings;
    }
}