namespace GitUI;

public static class FormExtensions
{
    /// <summary>
    ///  Brings the window to the front and activates it. Needed on drag drop and on button click from Windows Jump List.
    /// </summary>
    public static void ForceActivate(this Form? form)
    {
        if (form is null)
        {
            return;
        }

        form.TopMost = true;
        form.BringToFront();
        form.TopMost = false;
        form.Activate();
    }
}
