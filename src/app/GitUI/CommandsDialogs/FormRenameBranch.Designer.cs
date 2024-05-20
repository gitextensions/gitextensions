namespace GitUI.CommandsDialogs
{
    partial class FormRenameBranch
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
            Ok = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            BranchNameTextBox = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Ok.AutoSize = true;
            Ok.ForeColor = SystemColors.ControlText;
            Ok.Location = new Point(422, 7);
            Ok.Name = "Ok";
            Ok.Size = new Size(59, 28);
            Ok.TabIndex = 5;
            Ok.Text = "Rename";
            Ok.UseCompatibleTextRendering = true;
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(Ok, 2, 0);
            tableLayoutPanel1.Controls.Add(BranchNameTextBox, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(484, 42);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(3, 12);
            label1.Name = "label1";
            label1.Size = new Size(57, 18);
            label1.TabIndex = 3;
            label1.Text = "New name";
            label1.UseCompatibleTextRendering = true;
            // 
            // BranchNameTextBox
            // 
            BranchNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            BranchNameTextBox.Location = new Point(66, 10);
            BranchNameTextBox.Name = "BranchNameTextBox";
            BranchNameTextBox.Size = new Size(350, 21);
            BranchNameTextBox.TabIndex = 4;
            BranchNameTextBox.Leave += BranchNameTextBox_Leave;
            // 
            // FormRenameBranch
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(484, 42);
            Controls.Add(tableLayoutPanel1);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(400, 80);
            Name = "FormRenameBranch";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Rename branch";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button Ok;
        private TextBox BranchNameTextBox;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel1;
    }
}