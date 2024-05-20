namespace GitUI.ScriptsEngine
{
    partial class SimplePrompt
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
            btnOk = new Button();
            txtUserInput = new TextBox();
            labelInput = new Label();
            SuspendLayout();
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(247, 50);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 0;
            btnOk.Text = "&OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // txtUserInput
            // 
            txtUserInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUserInput.Location = new Point(12, 21);
            txtUserInput.Name = "txtUserInput";
            txtUserInput.Size = new Size(310, 23);
            txtUserInput.TabIndex = 1;
            txtUserInput.KeyPress += txtUserInput_KeyPress;
            // 
            // labelInput
            // 
            labelInput.AutoSize = true;
            labelInput.Location = new Point(10, 5);
            labelInput.Name = "labelInput";
            labelInput.Size = new Size(141, 15);
            labelInput.TabIndex = 2;
            labelInput.Text = "Please specify your input:";
            // 
            // SimplePrompt
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(334, 76);
            Controls.Add(labelInput);
            Controls.Add(txtUserInput);
            Controls.Add(btnOk);
            Name = "SimplePrompt";
            Text = "User input";
            Shown += SimplePrompt_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnOk;
        private TextBox txtUserInput;
        private Label labelInput;
    }
}
