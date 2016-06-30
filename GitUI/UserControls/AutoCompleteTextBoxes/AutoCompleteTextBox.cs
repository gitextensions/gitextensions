using System;
using System.Windows.Forms;

namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    // suppose to be an abstract class, but Winforms designer doesn't like it
    public class AutoCompleteTextBox : UserControl
    {
        protected AutoCompleteTextBox()
        {
        }

        public new event EventHandler TextChanged;
        public new event KeyEventHandler KeyDown;
        public new event PreviewKeyDownEventHandler PreviewKeyDown;


        protected void RaiseTextChangedEvent(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(this, e);
            }
        }

        protected void RaiseKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (KeyDown != null)
            {
                KeyDown(this, e);
            }
        }

        protected void RaisePreviewKeyDownEvent(object sender, PreviewKeyDownEventArgs e)
        {
            if (PreviewKeyDown != null)
            {
                PreviewKeyDown.Invoke(this, e);
            }
        }

        /// <summary>
        /// Should be abstract instead of virtual, but Winforms designer doesn't like it
        /// </summary>
        public virtual AutoCompleteMode AutoCompleteMode { get; set; }
        public virtual AutoCompleteStringCollection AutoCompleteCustomSource { get; set; }
        public virtual AutoCompleteSource AutoCompleteSource { get; set; }
    }
}
