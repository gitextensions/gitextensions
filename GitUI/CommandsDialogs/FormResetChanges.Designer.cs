namespace GitUI.CommandsDialogs
{
    partial class FormResetChanges
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
            label1 = new Label();
            btnReset = new Button();
            btnCancel = new Button();
            cbDeleteNewFilesAndDirectories = new CheckBox();
            label2 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 9);
            label1.Name = "label1";
            label1.Size = new Size(271, 16);
            label1.TabIndex = 0;
            label1.Text = "Are you sure you want to reset your changes?";
            // 
            // btnReset
            // 
            btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReset.Location = new Point(261, 3);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(95, 25);
            btnReset.TabIndex = 2;
            btnReset.Text = "R&eset";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(362, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(95, 25);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // cbDeleteNewFilesAndDirectories
            // 
            cbDeleteNewFilesAndDirectories.AutoSize = true;
            cbDeleteNewFilesAndDirectories.Location = new Point(10, 58);
            cbDeleteNewFilesAndDirectories.Name = "cbDeleteNewFilesAndDirectories";
            cbDeleteNewFilesAndDirectories.Size = new Size(251, 20);
            cbDeleteNewFilesAndDirectories.TabIndex = 3;
            cbDeleteNewFilesAndDirectories.Text = "Also delete &new files and/or directories";
            cbDeleteNewFilesAndDirectories.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 24);
            label2.Name = "label2";
            label2.Size = new Size(232, 16);
            label2.TabIndex = 4;
            label2.Text = "This will delete any uncommitted work.";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(btnCancel);
            flowLayoutPanel1.Controls.Add(btnReset);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(0, 84);
            flowLayoutPanel1.Margin = new Padding(2, 2, 2, 2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(460, 31);
            flowLayoutPanel1.TabIndex = 5;
            // 
            // FormResetChanges
            // 
            AcceptButton = btnCancel;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            CancelButton = btnCancel;
            ClientSize = new Size(460, 115);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(cbDeleteNewFilesAndDirectories);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormResetChanges";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reset changes";
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private Button btnReset;
        private Button btnCancel;
        private CheckBox cbDeleteNewFilesAndDirectories;
        private Label label2;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
