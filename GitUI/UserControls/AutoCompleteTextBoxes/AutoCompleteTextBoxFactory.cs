using GitCommands.Utils;

namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    internal static class AutoCompleteTextBoxFactory
    {
        public static AutoCompleteTextBox Create()
        {
#if !__MonoCS__
            return new WpfAutoCompleteTextBoxHost();
#else
            return new WinformsAutoCompleteTextBox();
#endif
        }
    }
}