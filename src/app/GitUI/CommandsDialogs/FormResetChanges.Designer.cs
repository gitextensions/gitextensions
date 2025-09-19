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
            txtMessage = new TextBox();
            btnReset = new Button();
            btnCancel = new Button();
            cbDeleteNewFilesAndDirectories = new CheckBox();
            label2 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtMessage
            // 
            txtMessage.BackColor = SystemColors.Control;
            txtMessage.BorderStyle = BorderStyle.None;
            txtMessage.Location = new Point(12, 12);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.ReadOnly = true;
            txtMessage.ScrollBars = ScrollBars.Vertical;
            txtMessage.Size = new Size(436, 60);
            txtMessage.TabIndex = 0;
            txtMessage.Text = "Are you sure you want to reset your changes?";
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 78);
            label2.Name = "label2";
            label2.Size = new Size(232, 16);
            label2.TabIndex = 4;
            label2.Text = "This will delete any uncommitted work.";
            // 
            // cbDeleteNewFilesAndDirectories
            // 
            cbDeleteNewFilesAndDirectories.AutoSize = true;
            cbDeleteNewFilesAndDirectories.Location = new Point(12, 97);
            cbDeleteNewFilesAndDirectories.Name = "cbDeleteNewFilesAndDirectories";
            cbDeleteNewFilesAndDirectories.Size = new Size(251, 20);
            cbDeleteNewFilesAndDirectories.TabIndex = 3;
            cbDeleteNewFilesAndDirectories.Text = "Also delete &new files and/or directories";
            cbDeleteNewFilesAndDirectories.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(btnCancel);
            flowLayoutPanel1.Controls.Add(btnReset);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(0, 123);
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
            CancelButton = btnCancel;
            ClientSize = new Size(460, 154);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(txtMessage);
            Controls.Add(label2);
            Controls.Add(cbDeleteNewFilesAndDirectories);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(476, 193);
            Name = "FormResetChanges";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reset changes";
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox txtMessage;
        private Button btnReset;
        private Button btnCancel;
        private CheckBox cbDeleteNewFilesAndDirectories;
        private Label label2;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
