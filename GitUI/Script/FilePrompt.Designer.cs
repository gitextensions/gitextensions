namespace GitUI.Script
{
    partial class FilePrompt
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilePrompt));
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Browse = new System.Windows.Forms.Button();
            this.txt_FilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(487, 56);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(94, 29);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "&OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Browse
            // 
            this.btn_Browse.Location = new System.Drawing.Point(487, 19);
            this.btn_Browse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Browse.Name = "btn_Browse";
            this.btn_Browse.Size = new System.Drawing.Size(94, 29);
            this.btn_Browse.TabIndex = 0;
            this.btn_Browse.Text = "Browse";
            this.btn_Browse.UseVisualStyleBackColor = true;
            this.btn_Browse.Click += new System.EventHandler(this.btn_Browse_Click);
            // 
            // txt_FilePath
            // 
            this.txt_FilePath.Location = new System.Drawing.Point(15, 26);
            this.txt_FilePath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txt_FilePath.Name = "txt_FilePath";
            this.txt_FilePath.Size = new System.Drawing.Size(464, 22);
            this.txt_FilePath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Please choose a file:";
            // 
            // FilePrompt
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(594, 91);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_FilePath);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.btn_Browse);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FilePrompt";
            this.Text = "FilePrompt";
            this.Shown += new System.EventHandler(this.FilePrompt_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Browse;
        private System.Windows.Forms.TextBox txt_FilePath;
        private System.Windows.Forms.Label label1;
    }
}