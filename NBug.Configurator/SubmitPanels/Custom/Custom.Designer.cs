namespace NBug.Configurator.SubmitPanels.Custom
{
    partial class Custom
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Custom));
            this.exampleCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // exampleCode
            // 
            this.exampleCode.BackColor = System.Drawing.SystemColors.Control;
            this.exampleCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.exampleCode.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.exampleCode.Location = new System.Drawing.Point(3, 3);
            this.exampleCode.Multiline = true;
            this.exampleCode.Name = "exampleCode";
            this.exampleCode.ReadOnly = true;
            this.exampleCode.Size = new System.Drawing.Size(454, 211);
            this.exampleCode.TabIndex = 44;
            this.exampleCode.TabStop = false;
            this.exampleCode.Text = resources.GetString("exampleCode.Text");
            // 
            // Custom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.exampleCode);
            this.Name = "Custom";
            this.Size = new System.Drawing.Size(475, 231);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox exampleCode;
    }
}
