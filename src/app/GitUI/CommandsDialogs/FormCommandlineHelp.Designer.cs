namespace GitUI.CommandsDialogs
{
    partial class FormCommandlineHelp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCommandlineHelp));
            _NO_TRANSLATE_commands = new Label();
            label1 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // _NO_TRANSLATE_commands
            // 
            _NO_TRANSLATE_commands.AutoSize = true;
            _NO_TRANSLATE_commands.Location = new Point(3, 77);
            _NO_TRANSLATE_commands.Name = "_NO_TRANSLATE_commands";
            _NO_TRANSLATE_commands.Padding = new Padding(10, 0, 10, 10);
            _NO_TRANSLATE_commands.Size = new Size(337, 789);
            _NO_TRANSLATE_commands.TabIndex = 0;
            _NO_TRANSLATE_commands.Text = resources.GetString("_NO_TRANSLATE_commands.Text");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(10);
            label1.Size = new Size(316, 77);
            label1.TabIndex = 1;
            label1.Text = "Supported commandline arguments for\r\ngitex.cmd / gitex (located in the same folde" +
    "r as GitExtensions.exe):";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(_NO_TRANSLATE_commands);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(394, 662);
            flowLayoutPanel1.TabIndex = 2;
            flowLayoutPanel1.WrapContents = false;
            // 
            // FormCommandlineHelp
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(394, 662);
            Controls.Add(flowLayoutPanel1);
            MaximizeBox = false;
            MinimumSize = new Size(410, 700);
            Name = "FormCommandlineHelp";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Commandline usage";
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label _NO_TRANSLATE_commands;
        private Label label1;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}