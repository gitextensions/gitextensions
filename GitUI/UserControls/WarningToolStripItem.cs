using System;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI
{
    public class WarningToolStripItem : ToolStripButton
    {
        private readonly Timer _blinkTimer;
        private int _counter;

        public WarningToolStripItem()
        {
            _counter = 0;
            Width = 200;
            Height = 20;
            RightToLeft = RightToLeft.No;
            _blinkTimer = new Timer { Interval = 150, Enabled = true };
            _blinkTimer.Tick += _blinkTimer_Tick;
            _blinkTimer.Start();
        }

        private void _blinkTimer_Tick(object sender, EventArgs e)
        {
            var warningColor = AppSettings.BranchColor;
            if (BackColor == warningColor && Parent != null)
            {
                BackColor = Parent.BackColor;
            }
            else
            {
                BackColor = warningColor;
            }

            _counter++;

            if (_counter > 20)
            {
                _blinkTimer.Stop();
                BackColor = warningColor;
            }

            this.SetForeColorForBackColor();
        }
    }
}
