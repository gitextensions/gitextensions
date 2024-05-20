namespace GitExtensions.Plugins.CreateLocalBranches
{
    partial class CreateLocalBranchesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateLocalBranchesForm));
            button1 = new Button();
            _NO_TRANSLATE_Remote = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            button1.DialogResult = DialogResult.OK;
            button1.Location = new Point(66, 40);
            button1.Margin = new Padding(4, 4, 4, 4);
            button1.Name = "button1";
            button1.Size = new Size(377, 30);
            button1.TabIndex = 0;
            button1.Text = "Create local tracking branches";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // _NO_TRANSLATE_Remote
            // 
            _NO_TRANSLATE_Remote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _NO_TRANSLATE_Remote.Location = new Point(321, 8);
            _NO_TRANSLATE_Remote.Margin = new Padding(4, 4, 4, 4);
            _NO_TRANSLATE_Remote.Name = "_NO_TRANSLATE_Remote";
            _NO_TRANSLATE_Remote.Size = new Size(176, 27);
            _NO_TRANSLATE_Remote.TabIndex = 2;
            _NO_TRANSLATE_Remote.Text = "origin";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 11);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(267, 20);
            label1.TabIndex = 1;
            label1.Text = "Remote to create tracking branches for";
            // 
            // CreateLocalBranchesForm
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(518, 79);
            Controls.Add(_NO_TRANSLATE_Remote);
            Controls.Add(label1);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            Name = "CreateLocalBranchesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create local tracking branches";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button button1;
        private TextBox _NO_TRANSLATE_Remote;
        private Label label1;
    }
}
