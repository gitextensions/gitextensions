using System.Drawing;
using System.Reflection;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeFlatButtonTests
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Source_flat_button_focus_renderer_should_separate_solid_light_and_dotted_dark_cues(bool dark)
    {
        using Font font = new("Segoe UI", 9);
        using Bitmap painted = new(26, 26);
        painted.SetResolution(96, 96);
        Color background = dark ? Color.FromArgb(51, 51, 51) : SystemColors.Control;
        using Graphics graphics = Graphics.FromImage(painted);
        graphics.Clear(background);
        if (dark)
        {
            Type rendererType = typeof(Button).Assembly.GetType("System.Windows.Forms.FlatButtonDarkModeRenderer", throwOnError: true)!;
            object renderer = Activator.CreateInstance(rendererType, nonPublic: true)!;
            MethodInfo focusMethod = rendererType.GetMethod("DrawFocusIndicator", BindingFlags.Instance | BindingFlags.Public)!;
            focusMethod.Invoke(renderer, [graphics, new Rectangle(0, 0, 26, 26), false]);
        }
        else
        {
            Type adapterType = typeof(Button).Assembly.GetType("System.Windows.Forms.ButtonInternal.ButtonFlatAdapter", throwOnError: true)!;
            MethodInfo layoutMethod = adapterType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(method => method.Name == "PaintFlatLayout");
            object options = layoutMethod.Invoke(null,
                [true, false, 0, new Rectangle(0, 0, 26, 26), new Padding(2), false, font, string.Empty, true, ContentAlignment.MiddleCenter, RightToLeft.No])!;
            object layout = options.GetType().GetMethod("Layout", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(options, null)!;
            Rectangle focus = (Rectangle)layout.GetType().GetField("Focus", BindingFlags.Instance | BindingFlags.Public)!.GetValue(layout)!;
            focus.Should().Be(new Rectangle(3, 3, 20, 20));
            using PaintEventArgs paint = new(graphics, new Rectangle(0, 0, 26, 26));
            Type baseAdapter = adapterType.BaseType!;
            baseAdapter.GetMethod("DrawFlatFocus", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, [paint, focus, SystemColors.ControlDark]);
        }

        Color[] top = Enumerable.Range(3, 20).Select(x => painted.GetPixel(x, 3)).ToArray();
        int paintedPixels = top.Count(color => color.ToArgb() != background.ToArgb());
        paintedPixels.Should().Be(dark ? 10 : 20, "dark ControlPaint uses a dotted cue; light DrawFlatFocus uses a continuous native rectangle");
        painted.GetPixel(2, 3).ToArgb().Should().Be(background.ToArgb());
        TestContext.Out.WriteLine($"inputMode=frameworkFocusRenderer dark={dark} topRow={string.Join(',', top.Select(color => color.ToArgb().ToString("X8")))} solidFocusBounds=3,3,20,20");
    }

    [Test]
    [TestCase(false, "normal", 0xF0F0F0, 0xF0F0F0)]
    [TestCase(false, "hover", 0xD8D8D8, 0xD8D8D8)]
    [TestCase(false, "pressed", 0xE5E5E5, 0xE5E5E5)]
    [TestCase(false, "disabled", 0xF0F0F0, 0xF0F0F0)]
    [TestCase(true, "normal", 0x333333, 0x9B9B9B)]
    [TestCase(true, "hover", 0x454545, 0x9B9B9B)]
    [TestCase(true, "pressed", 0x666666, 0xA0A0A0)]
    [TestCase(true, "disabled", 0x333333, 0x373737)]
    public void Source_search_button_adapter_should_paint_flat_client_fill_and_outline_without_toolbar_roles(
        bool dark, string state, int fillRgb, int edgeRgb)
    {
        // Reproduce the source btnSearch Designer configuration, including its actual
        // image. Invoke the real framework adapter, not a mocked palette. This is an
        // adapter-state probe, not evidence of physical pointer input or window focus.
        using Button button = new()
        {
            Size = new Size(26, 26),
            Padding = new Padding(2),
            Margin = Padding.Empty,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = true,
            Image = GitUI.Properties.Images.Preview,
            Enabled = state != "disabled",
        };
        button.FlatAppearance.BorderSize = 0;
        button.CreateControl();
        button.DeviceDpi.Should().Be(96);
        button.FlatAppearance.MouseOverBackColor.IsEmpty.Should().BeTrue();
        button.FlatAppearance.MouseDownBackColor.IsEmpty.Should().BeTrue();
        using Bitmap painted = new(button.Width, button.Height);
        painted.SetResolution(96, 96);
        using Graphics graphics = Graphics.FromImage(painted);
        graphics.Clear(Color.Magenta);
        using PaintEventArgs paint = new(graphics, button.ClientRectangle);
        Type adapterType = typeof(Button).Assembly.GetType(
            $"System.Windows.Forms.ButtonInternal.{(dark ? "ButtonDarkModeAdapter" : "ButtonFlatAdapter")}", throwOnError: true)!;
        object adapter = Activator.CreateInstance(adapterType, BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null, args: [button], culture: null)!;
        string paintMethod = state switch
        {
            "hover" => "PaintOver",
            "pressed" => "PaintDown",
            _ => "PaintUp",
        };
        MethodInfo method = adapterType.GetMethod(paintMethod, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(adapterType.FullName, paintMethod);
        method.Invoke(adapter, [paint, CheckState.Unchecked]);

        Color fill = Color.FromArgb(unchecked((int)0xFF000000) | fillRgb);
        Color edge = Color.FromArgb(unchecked((int)0xFF000000) | edgeRgb);
        TestContext.Out.WriteLine($"inputMode=frameworkAdapter state={state} dark={dark} size={button.Size} backdrop=#{button.BackColor.ToArgb():X8} middleRow={string.Join(',', Enumerable.Range(0, 26).Select(x => painted.GetPixel(x, 13).ToArgb().ToString("X8")))} upperColumn={string.Join(',', Enumerable.Range(0, 5).Select(y => painted.GetPixel(13, y).ToArgb().ToString("X8")))}");
        for (int y = 0; y < 5; y++)
        {
            TestContext.Out.WriteLine($"cornerRow{y}={string.Join(',', Enumerable.Range(0, 5).Select(x => painted.GetPixel(x, y).ToArgb().ToString("X8")))}");
        }

        Color firstPixel = dark ? BlendEdge(fill, button.BackColor, 2) : fill;
        painted.GetPixel(0, 13).ToArgb().Should().Be(firstPixel.ToArgb());
        painted.GetPixel(1, 13).ToArgb().Should().Be(edge.ToArgb());
        painted.GetPixel(2, 13).ToArgb().Should().Be(fill.ToArgb());
        painted.GetPixel(24, 13).ToArgb().Should().Be(fill.ToArgb());
        painted.GetPixel(25, 13).ToArgb().Should().Be((dark ? edge : fill).ToArgb());
        painted.GetPixel(0, 0).ToArgb().Should().Be((dark ? BlendEdge(fill, button.BackColor, 4) : fill).ToArgb());
    }

    private static Color BlendEdge(Color fill, Color backdrop, int divisor) => Color.FromArgb(
        (fill.R + (backdrop.R * (divisor - 1)) + (divisor / 2)) / divisor,
        (fill.G + (backdrop.G * (divisor - 1)) + (divisor / 2)) / divisor,
        (fill.B + (backdrop.B * (divisor - 1)) + (divisor / 2)) / divisor);
}
