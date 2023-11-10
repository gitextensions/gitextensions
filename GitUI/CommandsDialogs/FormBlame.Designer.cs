
namespace GitUI.CommandsDialogs
{
    partial class FormBlame
    {
        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            blameControl1 = new GitUI.Blame.BlameControl();
            SuspendLayout();
            // 
            // blameControl1
            // 
            blameControl1.Dock = DockStyle.Fill;
            blameControl1.Location = new Point(0, 0);
            blameControl1.Margin = new Padding(4);
            blameControl1.Name = "blameControl1";
            blameControl1.Size = new Size(784, 762);
            blameControl1.TabIndex = 0;
            // 
            // FormBlame
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(784, 762);
            Controls.Add(blameControl1);
            Name = "FormBlame";
            StartPosition = FormStartPosition.CenterParent;
            Text = "File History";
            Load += FormBlameLoad;
            ResumeLayout(false);
        }

        #endregion

        private Blame.BlameControl blameControl1;
    }
}