using System;
using System.Drawing;
using System.Windows.Forms;

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
            _blinkTimer = new Timer { Interval = 150, Enabled = true };
            _blinkTimer.Tick += _blinkTimer_Tick;
            _blinkTimer.Start();
        }

        private void _blinkTimer_Tick(object sender, EventArgs e)
        {
            if (BackColor == Color.Salmon && Parent != null)
            {
                BackColor = Parent.BackColor;
            }
            else
            {
                BackColor = Color.Salmon;
            }

            _counter++;

            if (_counter > 20)
            {
                _blinkTimer.Stop();
                BackColor = Color.Salmon;
            }
        }
    }
}
