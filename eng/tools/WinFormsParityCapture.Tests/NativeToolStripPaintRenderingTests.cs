using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Theming;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Measures original ordinary-button and submenu-arrow renderers without creating controls.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class NativeToolStripPaintRenderingTests
{
    private const int ToolBarButtonPart = 1;
    private const int ArrowHorizontalOffset = 2;
    private const int ArrowVerticalOffset = 4;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private enum SourceToolBarState
    {
        Normal = 1,
        Hot,
        Pressed,
        Disabled,
        Checked,
    }

    private static IEnumerable<(string Theme, bool UseSystemVisualStyle)> RendererCases()
    {
        yield return ("light", true);
        yield return ("light", false);
        yield return ("dark", false);
        yield return ("custom", false);
    }

    private static IEnumerable<TestCaseData> ButtonCases()
    {
        (string Name, CheckState Check, bool Enabled, bool Selected, bool Pressed)[] states =
        [
            ("normal", CheckState.Unchecked, true, false, false),
            ("selected", CheckState.Unchecked, true, true, false),
            ("pressed", CheckState.Unchecked, true, true, true),
            ("disabled", CheckState.Unchecked, false, false, false),
            ("checked", CheckState.Checked, true, false, false),
            ("checked-selected", CheckState.Checked, true, true, false),
            ("checked-pressed", CheckState.Checked, true, true, true),
            ("checked-disabled", CheckState.Checked, false, false, false),
            ("indeterminate", CheckState.Indeterminate, true, false, false),
        ];
        foreach ((string theme, bool useSystemVisualStyle) in RendererCases())
        {
            foreach ((string name, CheckState check, bool enabled, bool selected, bool pressed) in states)
            {
                foreach (Size size in new[] { new Size(23, 22), new Size(23, 23), new Size(32, 24) })
                {
                    yield return new TestCaseData(theme, useSystemVisualStyle, name, check, enabled, selected, pressed, size.Width, size.Height);
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> ArrowCases()
    {
        foreach ((string theme, bool useSystemVisualStyle) in RendererCases())
        {
            foreach (ArrowDirection direction in new[] { ArrowDirection.Left, ArrowDirection.Right })
            {
                foreach (string shape in new[] { "even", "odd-offset", "source-context-row", "short", "clipped" })
                {
                    foreach (string state in new[] { "normal", "selected", "disabled" })
                    {
                        yield return new TestCaseData(theme, useSystemVisualStyle, direction, shape, state);
                    }
                }
            }
        }
    }

    /// <summary>
    ///  Compares actual source background painting with independently dispatched native parts and source palette formulas.
    /// </summary>
    [TestCaseSource(nameof(ButtonCases))]
    public void DrawButtonBackground_should_preserve_checked_state_precedence_native_parts_and_original_border_bounds(
        string themeName, bool useSystemVisualStyle, string stateName, CheckState checkState,
        bool enabled, bool selected, bool pressed, int width, int height)
    {
        Theme theme = LoadToolTheme(themeName);
        SystemColorMode previousMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(theme.SystemColorMode);
            Application.IsDarkModeEnabled.Should().Be(theme.SystemColorMode == SystemColorMode.Dark);
            Application.RenderWithVisualStyles.Should().BeTrue();
            SystemInformation.HighContrast.Should().BeFalse();
            using ProbeButton item = new(selected, pressed)
            {
                AutoSize = false,
                Size = new Size(width, height),
                CheckState = checkState,
                Enabled = enabled,
                BackColor = Color.Transparent,
            };
            AssertOwnerlessNativeItem(item);
            ToolStripRenderer renderer = CreateOriginalRenderer(useSystemVisualStyle);
            AssertNoRendererOverride(renderer);
            ToolStripProfessionalRenderer? professional = renderer as ToolStripProfessionalRenderer;
            bool useSystemColors = professional is not null && ReadUseSystemColors(professional);
            SourceToolBarState systemState = GetSourceSystemState(checkState, enabled, selected, pressed);
            string directory = BitmapPaintProbe.CreateDirectory();
            List<object> rendered = [];
            List<(string Actual, string Expected)> hashes = [];
            foreach ((string role, Color backdrop) in new[]
            {
                ("black", Color.Black),
                ("white", Color.White),
                ("theme", theme.GetColor(AppColor.PanelBackground)),
            })
            {
                using Bitmap actual = PaintMemoryBitmap(item.Size, backdrop, graphics =>
                    renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(graphics, item)));
                using Bitmap expected = PaintMemoryBitmap(item.Size, backdrop, graphics =>
                {
                    if (professional is null)
                    {
                        PaintSystemSource(graphics, item.Size, systemState);
                    }
                    else
                    {
                        PaintProfessionalSource(graphics, item.Size, professional.ColorTable, useSystemColors, checkState, selected, pressed);
                    }
                });
                string originalFile = $"original-button-{role}.png";
                string expectedFile = $"independent-source-{role}.png";
                actual.Save(Path.Combine(directory, originalFile), ImageFormat.Png);
                expected.Save(Path.Combine(directory, expectedFile), ImageFormat.Png);
                string actualHash = BitmapPaintProbe.PixelHash(actual);
                string expectedHash = BitmapPaintProbe.PixelHash(expected);
                hashes.Add((actualHash, expectedHash));
                rendered.Add(new
                {
                    role,
                    backdrop = BitmapPaintProbe.Argb(backdrop),
                    originalFile,
                    expectedFile,
                    actualHash,
                    expectedHash,
                    actualInk = BitmapPaintProbe.Ink(actual, backdrop),
                    pixels = BitmapPaintProbe.Pixels(actual),
                });
            }

            BitmapPaintProbe.WriteReport(directory, new
            {
                themeName,
                useSystemVisualStyle,
                actualDark = Application.IsDarkModeEnabled,
                renderer = renderer.GetType().FullName,
                stateName,
                checkState = checkState.ToString(),
                enabled,
                selected,
                pressed,
                width,
                height,
                deviceDpi = ReadDeviceDpi(item),
                systemState = systemState.ToString(),
                systemPart = ToolBarButtonPart,
                systemBorderBounds = new Rectangle(0, 0, width, height - 1),
                professionalBorderBounds = new Rectangle(0, 0, width - 1, height - 1),
                useSystemColors,
                palette = ReadPalette(professional?.ColorTable),
                rendered,
                settingsAccessed = false,
                captureMethod = "originalOwnerlessToolStripButtonRendererBitmapGraphics",
                themeSource = "consumer-owned installed CSS only; renderer choice models ThemeFix.UseSystemVisualStyle independently of native dark mode",
                scope = "native96 background renderer only; controlled Selected/Pressed virtual properties, not HWND/pointer interaction; no image/text/control paint",
                ownerlessBoundary = "checked branches do not consult owner; unchecked owner-background overpaint is equivalent only for the source inherited/equal BackColor and null BackgroundImage",
                sourceReference = "dotnet/winforms v10.0.14 ToolStripSystemRenderer.RenderItemInternal/GetToolBarState; ToolstripProfessionalRenderer.OnRenderButtonBackground and its three fill helpers",
            });
            foreach ((string actualHash, string expectedHash) in hashes)
            {
                actualHash.Should().Be(expectedHash, "the independent source branch must match all native pixels");
            }
        }
        finally
        {
            Application.SetColorMode(previousMode);
        }
    }

    /// <summary>
    ///  Retains the actual GDI row coverage for left/right arrows at integer, odd and clipped rectangle inputs.
    /// </summary>
    [TestCaseSource(nameof(ArrowCases))]
    public void DrawArrow_should_match_original_integer_center_direction_and_caller_rectangle(
        string themeName, bool useSystemVisualStyle, ArrowDirection direction, string shape, string stateName)
    {
        Theme theme = LoadToolTheme(themeName);
        SystemColorMode previousMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(theme.SystemColorMode);
            Application.IsDarkModeEnabled.Should().Be(theme.SystemColorMode == SystemColorMode.Dark);
            SystemInformation.HighContrast.Should().BeFalse();
            bool selected = stateName == "selected";
            bool enabled = stateName != "disabled";
            using ToolStripMenuItem item = new()
            {
                Enabled = enabled,
                RightToLeft = direction == ArrowDirection.Left ? RightToLeft.Yes : RightToLeft.No,
            };
            AssertOwnerlessNativeItem(item);
            ToolStripRenderer renderer = CreateOriginalRenderer(useSystemVisualStyle);
            AssertNoRendererOverride(renderer);
            (Rectangle arrowRectangle, Size canvas) = ArrowShape(shape, direction);
            Color callerArrowColor = !enabled ? SystemColors.ControlDark : selected ? SystemColors.HighlightText : SystemColors.MenuText;

            // The Professional renderer replaces the default color for every
            // dropdown item, even when it has no owner and is selected.
            Color arrowColor = useSystemVisualStyle ? callerArrowColor : enabled ? SystemColors.ControlText : SystemColors.ControlDark;
            Color backdrop = SystemColors.Menu;
            Point middle = new(arrowRectangle.X + (arrowRectangle.Width / 2), arrowRectangle.Y + (arrowRectangle.Height / 2));
            int directionSign = direction == ArrowDirection.Left ? -1 : 1;
            Point[] points =
            [
                new(middle.X - (directionSign * ArrowHorizontalOffset), middle.Y - ArrowVerticalOffset),
                new(middle.X - (directionSign * ArrowHorizontalOffset), middle.Y + ArrowVerticalOffset),
                new(middle.X + (directionSign * ArrowHorizontalOffset), middle.Y),
            ];
            if (shape == "clipped")
            {
                points.Any(point => point.X < 0 || point.Y < 0 || point.X >= canvas.Width || point.Y >= canvas.Height).Should().BeTrue();
            }

            Color observedArrowColor = Color.Empty;
            using Bitmap actual = PaintMemoryBitmap(canvas, backdrop, graphics =>
            {
                ToolStripArrowRenderEventArgs args = new(graphics, item, arrowRectangle, callerArrowColor, direction);
                renderer.DrawArrow(args);
                observedArrowColor = args.ArrowColor;
            });
            using Bitmap expected = PaintMemoryBitmap(canvas, backdrop, graphics =>
            {
                using Brush brush = new SolidBrush(arrowColor);
                graphics.FillPolygon(brush, points);
            });
            string directory = BitmapPaintProbe.CreateDirectory();
            actual.Save(Path.Combine(directory, "original-arrow.png"), ImageFormat.Png);
            expected.Save(Path.Combine(directory, "independent-source-polygon.png"), ImageFormat.Png);
            BitmapPaintProbe.WriteReport(directory, new
            {
                themeName,
                useSystemVisualStyle,
                actualDark = Application.IsDarkModeEnabled,
                renderer = renderer.GetType().FullName,
                stateName,
                enabled,
                selected,
                direction = direction.ToString(),
                shape,
                arrowRectangle,
                canvas,
                middle,
                points,
                deviceDpi = ReadDeviceDpi(item),
                callerArrowColor = BitmapPaintProbe.Argb(callerArrowColor),
                arrowColor = BitmapPaintProbe.Argb(arrowColor),
                observedArrowColor = BitmapPaintProbe.Argb(observedArrowColor),
                backdrop = BitmapPaintProbe.Argb(backdrop),
                actualHash = BitmapPaintProbe.PixelHash(actual),
                expectedHash = BitmapPaintProbe.PixelHash(expected),
                actualInk = BitmapPaintProbe.Ink(actual, backdrop),
                rowRuns = ReadRowRuns(actual, backdrop),
                pixels = BitmapPaintProbe.Pixels(actual),
                settingsAccessed = false,
                captureMethod = "originalOwnerlessToolStripArrowRendererBitmapGraphics",
                scope = "native96 arrow primitive and ownerless dropdown-item color override only; explicit caller rectangle/direction and state-derived input color; not a real dropdown layout or pointer-state proof",
                sourceReference = "dotnet/winforms v10.0.14 ToolStripRenderer.RenderArrowCore; ToolStripMenuItem.OnPaint; ToolstripProfessionalRenderer.OnRenderArrow",
            });
            observedArrowColor.ToArgb().Should().Be(arrowColor.ToArgb());
            BitmapPaintProbe.AssertPixels(actual, expected);
        }
        finally
        {
            Application.SetColorMode(previousMode);
        }
    }

    private static Bitmap PaintMemoryBitmap(Size size, Color backdrop, Action<Graphics> paint)
    {
        Bitmap bitmap = new(size.Width, size.Height, PixelFormat.Format24bppRgb);
        try
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DpiX.Should().Be(96);
            graphics.DpiY.Should().Be(96);
            ToolStripSeparatorPaintTests.PaintOnInitializedDeviceContext(graphics, size, backdrop, deviceContext =>
            {
                using Graphics memoryGraphics = Graphics.FromHdc(deviceContext);
                memoryGraphics.DpiX.Should().Be(96);
                memoryGraphics.DpiY.Should().Be(96);
                paint(memoryGraphics);
            });
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static void PaintSystemSource(Graphics graphics, Size size, SourceToolBarState state)
    {
        VisualStyleRenderer part = new("TOOLBAR", ToolBarButtonPart, (int)state);
        part.DrawBackground(graphics, new Rectangle(Point.Empty, size));
        if (state is SourceToolBarState.Hot or SourceToolBarState.Pressed or SourceToolBarState.Checked)
        {
            // GDI's solid rectangle excludes Right/Bottom. Keep the final item
            // row outside the source's Height-1 highlight frame.
            PaintOutline(graphics, new Rectangle(0, 0, size.Width, size.Height - 1), SystemColors.Highlight);
        }
    }

    private static void PaintProfessionalSource(Graphics graphics, Size size, ProfessionalColorTable colors,
        bool useSystemColors, CheckState checkState, bool selected, bool pressed)
    {
        bool checkedBranch = checkState != CheckState.Unchecked;
        if (!checkedBranch && !selected && !pressed)
        {
            return;
        }

        bool pressedFill = checkedBranch ? selected : pressed;
        Color begin = pressedFill ? colors.ButtonPressedGradientBegin : checkedBranch ? colors.ButtonCheckedGradientBegin : colors.ButtonSelectedGradientBegin;
        Color end = pressedFill ? colors.ButtonPressedGradientEnd : checkedBranch ? colors.ButtonCheckedGradientEnd : colors.ButtonSelectedGradientEnd;
        Color solid = pressedFill ? colors.ButtonPressedHighlight : checkedBranch ? colors.ButtonCheckedHighlight : colors.ButtonSelectedHighlight;
        Rectangle bounds = new(Point.Empty, size);
        using Brush brush = useSystemColors ? new SolidBrush(solid) : new LinearGradientBrush(bounds, begin, end, LinearGradientMode.Vertical);
        graphics.FillRectangle(brush, bounds);
        PaintOutline(graphics, bounds, colors.ButtonSelectedBorder);
    }

    private static void PaintOutline(Graphics graphics, Rectangle bounds, Color color)
    {
        using Brush brush = new SolidBrush(color);
        graphics.FillRectangle(brush, bounds.Left, bounds.Top, bounds.Width, 1);
        graphics.FillRectangle(brush, bounds.Left, bounds.Bottom - 1, bounds.Width, 1);
        graphics.FillRectangle(brush, bounds.Left, bounds.Top, 1, bounds.Height);
        graphics.FillRectangle(brush, bounds.Right - 1, bounds.Top, 1, bounds.Height);
    }

    private static SourceToolBarState GetSourceSystemState(CheckState checkState, bool enabled, bool selected, bool pressed)
        => checkState != CheckState.Unchecked ? selected ? SourceToolBarState.Hot : SourceToolBarState.Checked
            : pressed ? SourceToolBarState.Pressed : selected ? SourceToolBarState.Hot
            : enabled ? SourceToolBarState.Normal : SourceToolBarState.Disabled;

    private static (Rectangle Rectangle, Size Canvas) ArrowShape(string shape, ArrowDirection direction)
        => shape switch
        {
            "even" => (new Rectangle(0, 0, 10, 20), new Size(18, 28)),
            "odd-offset" => (new Rectangle(3, 5, 11, 21), new Size(22, 34)),
            "source-context-row" => (new Rectangle(direction == ArrowDirection.Left ? 8 : 247, 1, 10, 20), new Size(265, 22)),
            "short" => (new Rectangle(2, 1, 10, 11), new Size(18, 13)),
            "clipped" => (new Rectangle(-5, -8, 10, 20), new Size(10, 14)),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };

    private static object[] ReadRowRuns(Bitmap bitmap, Color backdrop)
        => Enumerable.Range(0, bitmap.Height).Select(y => new
        {
            y,
            xs = Enumerable.Range(0, bitmap.Width).Where(x => bitmap.GetPixel(x, y).ToArgb() != backdrop.ToArgb()).ToArray(),
        }).Where(row => row.xs.Length > 0).Cast<object>().ToArray();

    private static ToolStripRenderer CreateOriginalRenderer(bool useSystemVisualStyle)
    {
        if (useSystemVisualStyle)
        {
            return new ToolStripExSystemRenderer();
        }

        Type type = typeof(ToolStripExSystemRenderer).Assembly.GetType("GitUI.ToolStripExProfessionalRenderer", throwOnError: true)
            ?? throw new TypeLoadException("GitUI.ToolStripExProfessionalRenderer");
        return Activator.CreateInstance(type) as ToolStripRenderer
            ?? throw new InvalidOperationException("The original internal renderer must be constructible without its owning control.");
    }

    private static void AssertOwnerlessNativeItem(ToolStripItem item)
    {
        item.Owner.Should().BeNull();
        item.GetCurrentParent().Should().BeNull();
        ReadDeviceDpi(item).Should().Be(96);
    }

    private static int ReadDeviceDpi(ToolStripItem item)
        => (int)(typeof(ToolStripItem).GetProperty("DeviceDpi", PrivateInstance | BindingFlags.Public)?.GetValue(item)
            ?? throw new MissingMemberException(typeof(ToolStripItem).FullName, "DeviceDpi"));

    private static void AssertNoRendererOverride(ToolStripRenderer renderer)
    {
        PropertyInfo property = typeof(ToolStripRenderer).GetProperty("RendererOverride", PrivateInstance)
            ?? throw new MissingMemberException(typeof(ToolStripRenderer).FullName, "RendererOverride");
        property.GetValue(renderer).Should().BeNull("contrast/low-resolution substitution is a separate source rendering context");
    }

    private static bool ReadUseSystemColors(ToolStripProfessionalRenderer renderer)
        => (bool)(typeof(ToolStripProfessionalRenderer).GetProperty("UseSystemColors", PrivateInstance)?.GetValue(renderer)
            ?? throw new MissingMemberException(typeof(ToolStripProfessionalRenderer).FullName, "UseSystemColors"));

    private static Dictionary<string, string> ReadPalette(ProfessionalColorTable? colors)
    {
        Dictionary<string, string> palette = new()
        {
            [nameof(SystemColors.Highlight)] = BitmapPaintProbe.Argb(SystemColors.Highlight),
            [nameof(SystemColors.Menu)] = BitmapPaintProbe.Argb(SystemColors.Menu),
            [nameof(SystemColors.MenuText)] = BitmapPaintProbe.Argb(SystemColors.MenuText),
            [nameof(SystemColors.HighlightText)] = BitmapPaintProbe.Argb(SystemColors.HighlightText),
            [nameof(SystemColors.ControlDark)] = BitmapPaintProbe.Argb(SystemColors.ControlDark),
        };
        if (colors is not null)
        {
            foreach (string property in new[]
            {
                nameof(ProfessionalColorTable.ButtonCheckedGradientBegin), nameof(ProfessionalColorTable.ButtonCheckedGradientEnd), nameof(ProfessionalColorTable.ButtonCheckedHighlight),
                nameof(ProfessionalColorTable.ButtonPressedGradientBegin), nameof(ProfessionalColorTable.ButtonPressedGradientEnd), nameof(ProfessionalColorTable.ButtonPressedHighlight),
                nameof(ProfessionalColorTable.ButtonSelectedGradientBegin), nameof(ProfessionalColorTable.ButtonSelectedGradientEnd), nameof(ProfessionalColorTable.ButtonSelectedHighlight),
                nameof(ProfessionalColorTable.ButtonSelectedBorder),
            })
            {
                Color color = (Color)(typeof(ProfessionalColorTable).GetProperty(property)?.GetValue(colors)
                    ?? throw new MissingMemberException(typeof(ProfessionalColorTable).FullName, property));
                palette[property] = BitmapPaintProbe.Argb(color);
            }
        }

        return palette;
    }

    private static Theme LoadToolTheme(string name)
    {
        if (name == "light")
        {
            return Theme.CreateDefaultTheme();
        }

        string root = Path.Combine(TestContext.CurrentContext.TestDirectory, "Themes");
        IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
        resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(root, call.Arg<string>()));
        ThemeLoader loader = new(resolver, new ThemeFileReader());
        return loader.LoadTheme(Path.Combine(root, name == "custom" ? "parity-custom.css" : "dark.css"), new ThemeId(name), []);
    }

    private sealed class ProbeButton(bool selected, bool pressed) : ToolStripButton
    {
        /// <summary>
        ///  Supplies the renderer input without creating an owning toolbar or synthesizing pointer input.
        /// </summary>
        public override bool Selected => selected;

        /// <summary>
        ///  Supplies the pressed branch independently of the selected branch.
        /// </summary>
        public override bool Pressed => pressed;
    }
}
