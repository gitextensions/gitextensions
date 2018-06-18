
namespace GitUI.CommandsDialogs
{
    partial class FormBlame
    {
        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.blameControl1 = new GitUI.Blame.BlameControl();
            this.SuspendLayout();
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
            this.ResumeLayout(false);
        }

        #endregion

        private Blame.BlameControl blameControl1;
    }
}