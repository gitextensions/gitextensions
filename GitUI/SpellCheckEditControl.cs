using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace GitUI
{
    public struct TextPos
    {
        public TextPos(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        public int Start;
        public int End;
    }

    public class CustomPaintTextBox : NativeWindow
    {
        private RichTextBox textBox;
        private Bitmap bitmap;
        private Graphics textBoxGraphics;
        private Graphics bufferGraphics;

        public List<TextPos> Lines = new List<TextPos>();


        // this is where we intercept the Paint event for the TextBox at the OS level   
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 15: // this is the WM_PAINT message   
                    // invalidate the TextBox so that it gets refreshed properly   
                    textBox.Invalidate();
                    // call the default win32 Paint method for the TextBox first   
                    base.WndProc(ref m);
                    // now use our code to draw extra stuff over the TextBox   
                    this.CustomPaint();
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        public CustomPaintTextBox(RichTextBox textBox)
        {
            this.textBox = textBox;
            // Start receiving messages (make sure you call ReleaseHandle on Dispose):   
            this.AssignHandle(textBox.Handle);
        }
        private void CustomPaint()
        {
            if (this.bitmap == null || (this.bitmap.Width != textBox.Width && this.bitmap.Height != textBox.Height))
            {
                this.bitmap = new Bitmap(textBox.Width, textBox.Height);
                this.bufferGraphics = Graphics.FromImage(this.bitmap);
                this.bufferGraphics.Clip = new Region(textBox.ClientRectangle);
                this.textBoxGraphics = Graphics.FromHwnd(textBox.Handle);
            }

            // get the index of the first visible line in the TextBox   
            int curPos = TextBoxAPIHelper.GetFirstVisibleLine(textBox);
            curPos = TextBoxAPIHelper.GetLineIndex(textBox, curPos);
            // clear the graphics buffer   
            bufferGraphics.Clear(Color.Transparent);

            // * Here’s where the magic happens   
            foreach (TextPos textPos in Lines)
            {

                Point start = textBox.GetPositionFromCharIndex(textPos.Start);
                Point end = textBox.GetPositionFromCharIndex(textPos.End);
                // The position above now points to the top left corner of the character.   
                // We need to account for the character height so the underlines go   
                // to the right place.   
                end.X += 1;
                start.Y += TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(textBox, textPos.Start);
                end.Y += TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(textBox, textPos.End);

                if (start.X == -1 || end.X == -1)
                    continue;

                // Draw the wavy underline.   
                //Multiline wiggle.....
                if (start.Y < end.Y - 3 || start.Y > end.Y + 3)
                {
                    if (start.X != end.X && start.Y < end.Y)
                    {
                        DrawWave(start, new Point(textBox.Width - 3, start.Y));
                        DrawWave(new Point(3, end.Y), end);
                    }
                }
                else
                    DrawWave(start, end);
            }


            // Now we just draw our internal buffer on top of the TextBox.   
            // Everything should be at the right place.   
            textBoxGraphics.DrawImageUnscaled(bitmap, 0, 0);
        }

        private void DrawWave(Point start, Point end)   
        {   
            Pen pen = Pens.Red;   
            if ((end.X - start.X) > 4)
            {   
                ArrayList pl = new ArrayList();   
                for (int i = start.X; i <= (end.X - 2); i += 4)   
                {   
                    pl.Add(new Point(i, start.Y));   
                    pl.Add(new Point(i + 2, start.Y + 2));   
                }   
                Point [] p = (Point[])pl.ToArray(typeof(Point));   
                bufferGraphics.DrawLines(pen, p);   
            }   
            else    
            {   
                bufferGraphics.DrawLine(pen, start, end);   
            }   
        } 
    }
}
