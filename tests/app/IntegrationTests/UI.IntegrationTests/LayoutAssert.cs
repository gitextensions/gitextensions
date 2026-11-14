namespace GitExtensions.UITests;

/// <summary>
///  Assertions that verify that a dialog fits its content after it has been laid out.
/// </summary>
/// <remarks>
///  <para>
///   The Windows "Text size" accessibility setting enlarges the system font without changing the
///   DPI, so none of the DPI scaling reacts to it and every size the designer recorded stays as it
///   is. These assertions therefore compare the bounds the layout produced against the size the
///   controls report they need, rather than against fixed pixel values, so that they hold at any
///   font size.
///  </para>
/// </remarks>
internal static class LayoutAssert
{
    /// <summary>
    ///  Asserts that no two visible children of <paramref name="container"/> overlap each other.
    /// </summary>
    public static void ChildrenDoNotOverlap(Control container)
    {
        List<Control> children = VisibleChildren(container);

        for (int i = 0; i < children.Count; i++)
        {
            for (int j = i + 1; j < children.Count; j++)
            {
                // Rectangle.IsEmpty is true only when BOTH dimensions are zero, not when the area
                // is zero - two rows that merely touch at a shared edge intersect in a rectangle
                // with one zero dimension and the other equal to the shared width/height, which
                // IsEmpty reports as false. Check the area directly instead.
                Rectangle overlap = Rectangle.Intersect(children[i].Bounds, children[j].Bounds);
                bool hasArea = overlap.Width > 0 && overlap.Height > 0;
                hasArea.Should().BeFalse($"{Describe(children[i])} must not overlap {Describe(children[j])}");
            }
        }
    }

    /// <summary>
    ///  Asserts that every visible child of <paramref name="container"/> is fully inside it.
    /// </summary>
    public static void ChildrenFitIntoContainer(Control container)
    {
        Rectangle display = container.DisplayRectangle;

        foreach (Control child in VisibleChildren(container))
        {
            display.Contains(child.Bounds).Should()
                .BeTrue($"{Describe(child)} must fit into {container.Name} {display}");
        }
    }

    /// <summary>
    ///  Asserts that <paramref name="control"/> is at least as large as the size it reports it
    ///  needs, i.e. that its caption is not cut off.
    /// </summary>
    public static void CaptionFits(Control control)
    {
        Size preferred = control.PreferredSize;

        control.Width.Should().BeGreaterThanOrEqualTo(preferred.Width, $"{Describe(control)} must fit its caption");
        control.Height.Should().BeGreaterThanOrEqualTo(preferred.Height, $"{Describe(control)} must fit its caption");
    }

    /// <summary>
    ///  Writes the bounds of <paramref name="container"/> and of its visible children to the test
    ///  output, so that a failure can be read without a debugger.
    /// </summary>
    public static void Report(Control container)
    {
        TestContext.Out.WriteLine($"{container.Name} {container.Bounds}, font {container.Font.Name} {container.Font.SizeInPoints}pt (height {container.Font.Height})");

        foreach (Control child in VisibleChildren(container))
        {
            TestContext.Out.WriteLine($"  {child.Name} {child.Bounds}, preferred {child.PreferredSize}");
        }
    }

    private static List<Control> VisibleChildren(Control container)
        => [.. container.Controls.OfType<Control>().Where(child => child.Visible)];

    private static string Describe(Control control)
        => $"{control.Name} ({control.GetType().Name}) {control.Bounds}";
}
