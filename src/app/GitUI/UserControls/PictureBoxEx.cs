namespace GitUI.UserControls;

/// <summary>
///  A PictureBox which handles exceptions from OnPaint.
/// </summary>
internal class PictureBoxEx : PictureBox
{
    public event EventHandler<Exception> PaintFailed;

    protected override void OnPaint(PaintEventArgs e)
    {
        try
        {
            base.OnPaint(e);
        }
        catch (Exception ex)
        {
            e.Graphics.Clear(SystemColors.Window);
            PaintFailed?.Invoke(this, ex);
        }
    }
}
