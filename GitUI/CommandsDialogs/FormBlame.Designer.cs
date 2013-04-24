
namespace GitUI.CommandsDialogs
{
    partial class FormBlame
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
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.blameControl1 = new GitUI.Blame.BlameControl();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // blameControl1
            // 
            this.blameControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blameControl1.Location = new System.Drawing.Point(0, 0);
            this.blameControl1.Margin = new System.Windows.Forms.Padding(4);
            this.blameControl1.Name = "blameControl1";
            this.blameControl1.Size = new System.Drawing.Size(784, 762);
            this.blameControl1.TabIndex = 0;
            // 
            // FormBlame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 762);
            this.Controls.Add(this.blameControl1);
            this.Name = "FormBlame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File History";
            this.Load += new System.EventHandler(this.FormBlameLoad);
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Diagnostics.EventLog eventLog1;
        private Blame.BlameControl blameControl1;
    }
}