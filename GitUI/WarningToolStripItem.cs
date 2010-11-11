using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GitUI
{
    public class WarningToolStripItem : ToolStripButton
    {
        private Timer _blinkTimer;
        private int _counter;

        public WarningToolStripItem()
        {
            _counter = 0;
            this.Width = 200;
            this.Height = 20;
            _blinkTimer = new Timer();
            _blinkTimer.Interval = 150;
            _blinkTimer.Enabled = true;
            _blinkTimer.Tick += new EventHandler(_blinkTimer_Tick);
            _blinkTimer.Start();
        }

        void _blinkTimer_Tick(object sender, EventArgs e)
        {
            if (BackColor == Color.Salmon && Parent != null)
                BackColor = Parent.BackColor;
            else
                BackColor = Color.Salmon;

            _counter++;

            if (_counter > 20)
            {
                _blinkTimer.Stop();
                BackColor = Color.Salmon;
            }
        }

    }
}
