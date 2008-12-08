namespace GitUI
{
    partial class FormPush
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPush));
            this.Output = new System.Windows.Forms.RichTextBox();
            this.BrowseSource = new System.Windows.Forms.Button();
            this.PushDestination = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Push = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Output
            // 
            this.Output.Location = new System.Drawing.Point(22, 46);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(562, 222);
            this.Output.TabIndex = 14;
            this.Output.Text = "";
            // 
            // BrowseSource
            // 
            this.BrowseSource.Location = new System.Drawing.Point(372, 17);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(104, 23);
            this.BrowseSource.TabIndex = 13;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSource_Click);
            // 
            // PushDestination
            // 
            this.PushDestination.Location = new System.Drawing.Point(119, 20);
            this.PushDestination.Name = "PushDestination";
            this.PushDestination.Size = new System.Drawing.Size(247, 20);
            this.PushDestination.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Destination";
            // 
            // Push
            // 
            this.Push.Location = new System.Drawing.Point(482, 17);
            this.Push.Name = "Push";
            this.Push.Size = new System.Drawing.Size(104, 23);
            this.Push.TabIndex = 15;
            this.Push.Text = "Push";
            this.Push.UseVisualStyleBackColor = true;
            this.Push.Click += new System.EventHandler(this.Push_Click);
            // 
            // FormPush
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 285);
            this.Controls.Add(this.Push);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.BrowseSource);
            this.Controls.Add(this.PushDestination);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPush";
            this.Text = "Push";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.TextBox PushDestination;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Push;
    }
}