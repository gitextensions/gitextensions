namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class FormDashboardCategoryTitle
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
            FlowLayoutPanel flpnlControls;
            btnOk = new Button();
            btnCancel = new Button();
            lblCategoryName = new Label();
            txtCategoryName = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            flpnlControls = new FlowLayoutPanel();
            flpnlControls.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // flpnlControls
            // 
            flpnlControls.Controls.Add(btnOk);
            flpnlControls.Controls.Add(btnCancel);
            flpnlControls.Dock = DockStyle.Bottom;
            flpnlControls.FlowDirection = FlowDirection.RightToLeft;
            flpnlControls.Location = new Point(0, 26);
            flpnlControls.Name = "flpnlControls";
            flpnlControls.Size = new Size(384, 34);
            flpnlControls.TabIndex = 4;
            flpnlControls.WrapContents = false;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnOk.AutoSize = true;
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(257, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(124, 25);
            btnOk.TabIndex = 0;
            btnOk.Text = "OK";
            btnOk.UseCompatibleTextRendering = true;
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += OkButton_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnCancel.AutoSize = true;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(176, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 25);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseCompatibleTextRendering = true;
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblCategoryName
            // 
            lblCategoryName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblCategoryName.AutoSize = true;
            lblCategoryName.Location = new Point(3, 4);
            lblCategoryName.Name = "lblCategoryName";
            lblCategoryName.Size = new Size(80, 18);
            lblCategoryName.TabIndex = 0;
            lblCategoryName.Text = "Category name";
            lblCategoryName.TextAlign = ContentAlignment.MiddleLeft;
            lblCategoryName.UseCompatibleTextRendering = true;
            // 
            // txtCategoryName
            // 
            txtCategoryName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCategoryName.Location = new Point(89, 3);
            txtCategoryName.Name = "txtCategoryName";
            txtCategoryName.Size = new Size(292, 21);
            txtCategoryName.TabIndex = 1;
            txtCategoryName.TextChanged += txtCategoryName_TextChanged;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(lblCategoryName, 0, 0);
            tableLayoutPanel1.Controls.Add(txtCategoryName, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(384, 27);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // FormDashboardCategoryTitle
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CancelButton = btnCancel;
            ClientSize = new Size(384, 60);
            Controls.Add(flpnlControls);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(400, 80);
            Name = "FormDashboardCategoryTitle";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Enter Caption";
            flpnlControls.ResumeLayout(false);
            flpnlControls.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label lblCategoryName;
        private TextBox txtCategoryName;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnOk;
        private Button btnCancel;
    }
}