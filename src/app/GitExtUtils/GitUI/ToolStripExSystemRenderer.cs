namespace GitUI;

public sealed class ToolStripExSystemRenderer : ToolStripSystemRenderer
{
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.ToolStrip!.GetMenuItemBackgroundFilter()?.ShouldRenderMenuItemBackground(e) != false)
        {
            base.OnRenderMenuItemBackground(e);
        }
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        if (e.ToolStrip is not IToolStripEx { DrawBorder: false })
        {
            // render border
            base.OnRenderToolStripBorder(e);
        }
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        ToolStripArrowScaler.RenderScaledArrow(e, base.OnRenderArrow);
    }

    protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
    {
        base.OnRenderSplitButtonBackground(e);

        // Unlike ToolStripDropDownButton (whose OnPaint calls DrawArrow directly), the system renderer
        // paints the split-button arrow inline at a fixed size, bypassing OnRenderArrow. Re-issue it
        // through DrawArrow so OnRenderArrow scales it to the icon size. The larger arrow fully covers
        // the small one already drawn by the base renderer. Skipped when no scaling is needed so the
        // native arrow is left untouched at the baseline icon size.
        if (e.Item is ToolStripSplitButton splitButton && ToolStripArrowScaler.NeedsScaling(splitButton))
        {
            Color arrowColor = splitButton.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;
            DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, splitButton, splitButton.DropDownButtonBounds, arrowColor, ArrowDirection.Down));
        }
    }
}
