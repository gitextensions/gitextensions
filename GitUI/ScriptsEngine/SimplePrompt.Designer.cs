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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimplePrompt));
            btn_OK = new Button();
            txt_UserInput = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // btn_OK
            // 
            btn_OK.Location = new Point(194, 47);
            btn_OK.Name = "btn_OK";
            btn_OK.Size = new Size(75, 23);
            btn_OK.TabIndex = 0;
            btn_OK.Text = "&OK";
            btn_OK.UseVisualStyleBackColor = true;
            btn_OK.Click += btn_OK_Click;
            // 
            // txt_UserInput
            // 
            txt_UserInput.Location = new Point(12, 21);
            txt_UserInput.Name = "txt_UserInput";
            txt_UserInput.Size = new Size(257, 20);
            txt_UserInput.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 5);
            label1.Name = "label1";
            label1.Size = new Size(127, 13);
            label1.TabIndex = 2;
            label1.Text = "Please specify your input:";
            // 
            // SimplePrompt
            // 
            AcceptButton = btn_OK;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(281, 73);
            Controls.Add(label1);
            Controls.Add(txt_UserInput);
            Controls.Add(btn_OK);
            Name = "SimplePrompt";
            Text = "SimplePrompt";
            Shown += SimplePrompt_Shown;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button btn_OK;
        private TextBox txt_UserInput;
        private Label label1;
    }
}
