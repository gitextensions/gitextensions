using System.Windows.Controls;

namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    class WpfAutoCompleteTextBoxEx : AutoCompleteBox
    {
        /// <summary>
        /// Override this method to enable the enter, esc and etc key.
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (!IsDropDownOpen)
            {
                base.OnKeyDown(e);
                e.Handled = false;
                return;
            }
            base.OnKeyDown(e);
        }
    }
}