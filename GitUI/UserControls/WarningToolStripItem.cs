using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary><see cref="ToolStripButton"/> which blinks when</summary>
    public class WarningToolStripItem : ToolStripButton
    {
        /// <summary>3 seconds</summary>
        static readonly double TotalBlinkDuration = 3000;
        /// <summary>150ms</summary>
        static readonly int BlinkInterval = 150;
        readonly Timer _blinkTimer = new Timer { Interval = BlinkInterval };
        int _counter;
        Color _offColor;

        public WarningToolStripItem()
        {
            Width = 200;
            Height = 20;

            _offColor = Parent != null ? Parent.BackColor : SystemColors.Control;
            _blinkTimer.Tick += BlinkOnTick;

            _blinkTimer.Start();
        }

        void BlinkOnTick(object s, EventArgs e)
        {
            ToggleOnOff(() =>
             {
                 BackColor = Color.Salmon;
             }, () =>
             {
                 BackColor = _offColor;
             });
        }

        /// <summary>Toggles between an 'on' and 'off' action.</summary>
        void ToggleOnOff(Action on, Action off, Action finish = null)
        {
            if (_counter % 2 == 0)
            {// even -> off
                off();
                Debug.WriteLine("off");
            }
            else
            {// odd -> on
                on();
                Debug.WriteLine("on");
            }

            _counter++;// increment

            if (_counter >= 20)
            {// enough blinks -> stop blinking
                Debug.WriteLine(">stop");
                _blinkTimer.Stop();
                _blinkTimer.Tick -= BlinkOnTick;
                if (finish != null)
                {
                    finish();
                }
                Debug.WriteLine(">done");
            }
        }
    }
}
