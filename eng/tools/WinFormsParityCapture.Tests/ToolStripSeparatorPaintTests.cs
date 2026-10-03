using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Theming;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripSeparatorPaintTests
{
    private const string ThemeClass = "TOOLBAR";
    private const int SeparatorPart = 5;
    private const int ProfessionalItemInset = 3;
    private const int ProfessionalItemTrim = 6;
    private const int ProfessionalLineInset = 2;

    [Test]
    [TestCase(false, false, 6, 25, true, false)]
    [TestCase(false, false, 6, 25, false, false)]
    [TestCase(false, false, 6, 25, true, true)]
    [TestCase(false, false, 9, 31, true, false)]
    [TestCase(false, false, 10, 18, false, true)]
    [TestCase(false, false, 6, 9, false, true)]
    [TestCase(false, false, 6, 4, true, false)]
    [TestCase(false, false, 6, 1, true, false)]
    [TestCase(false, false, 6, 5, true, false)]
    [TestCase(false, false, 5, 5, true, false)]
    [TestCase(false, false, 4, 5, true, false)]
    [TestCase(false, true, 6, 25, true, false)]
    [TestCase(false, true, 6, 25, false, false)]
    [TestCase(false, true, 6, 25, true, true)]
    [TestCase(false, true, 9, 31, true, false)]
    [TestCase(true, false, 6, 25, true, false)]
    [TestCase(true, false, 6, 25, false, true)]
    [TestCase(true, true, 6, 25, true, false)]
    [TestCase(true, true, 6, 25, false, false)]
    [TestCase(true, true, 6, 25, true, true)]
    [TestCase(true, true, 9, 31, true, false)]
    [TestCase(true, true, 10, 18, false, true)]
    [TestCase(true, true, 6, 9, false, true)]
    [TestCase(true, true, 6, 11, true, false)]
    [TestCase(true, true, 6, 4, true, false)]
    [TestCase(true, true, 6, 1, true, false)]
    public void Source_separator_should_follow_its_actual_renderer_and_native_theme_metadata(
        bool professional, bool dark, int width, int height, bool enabled, bool rightToLeft)
    {
        SystemColorMode originalMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.DoEvents();
            using Form host = new() { AutoScaleMode = AutoScaleMode.None, ShowInTaskbar = false };
            using ToolStripEx strip = CreateStrip(host);
            if (professional)
            {
                strip.RenderMode = ToolStripRenderMode.Professional;
            }

            using ToolStripSeparator separator = new()
            {
                AutoSize = false,
                Size = new Size(width, height),
                Margin = Padding.Empty,
                Enabled = enabled,
                RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
            };
            strip.Items.Add(separator);
            strip.PerformLayout();
            separator.Size = new Size(width, height);
            separator.Size.Should().Be(new Size(width, height));
            strip.DeviceDpi.Should().Be(96);
            SystemInformation.HighContrast.Should().BeFalse("these probes exercise the ordinary system/professional renderer, not its high-contrast substitute");
            strip.Renderer.Should().BeOfType(professional ? typeof(ToolStripProfessionalRenderer) : typeof(ToolStripExSystemRenderer));
            Color backdrop = strip.BackColor;
            string directory = CreateDiagnosticDirectory();
            using Bitmap actual = RenderSeparator(strip, separator, backdrop);
            using Bitmap expected = NewBitmap(separator.Size, backdrop);
            using Bitmap overBlack = RenderSeparator(strip, separator, Color.Black);
            using Bitmap overWhite = RenderSeparator(strip, separator, Color.White);
            object nativeTheme = QueryNativeTheme(strip, separator.Size, enabled ? 1 : 4);
            object? declaredOverride = professional ? null
                : typeof(ToolStripSystemRenderer).GetProperty("RendererOverride", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(strip.Renderer);
            using (Graphics graphics = Graphics.FromImage(expected))
            {
                if (strip.Renderer is ToolStripProfessionalRenderer renderer)
                {
                    AssertProfessionalColors(renderer.ColorTable);
                    PaintProfessionalSourceLines(graphics, separator, renderer.ColorTable);
                }
                else
                {
                    // The system renderer passes the complete item rectangle to this part.
                    // Its image/margins, not a screenshot-derived inset, determine the line.
                    VisualStyleRenderer partRenderer = new(ThemeClass, SeparatorPart, enabled ? 1 : 4);
                    PaintOnInitializedDeviceContext(graphics, separator.Size, backdrop, deviceContext =>
                    {
                        NativeRectangle bounds = new() { Right = separator.Width, Bottom = separator.Height };
                        DrawThemeBackground(partRenderer.Handle, deviceContext, SeparatorPart, enabled ? 1 : 4, ref bounds, 0).Should().Be(0);
                    });
                }
            }

            if (!professional && dark)
            {
                // The .NET10 System renderer's separator override calls the theme part
                // directly. Exercise the distinct dark renderer independently as well;
                // the availability of RendererOverride does not prove it was dispatched.
                Type rendererType = typeof(ToolStripRenderer).Assembly.GetType("System.Windows.Forms.ToolStripSystemDarkModeRenderer", throwOnError: true)!;
                ToolStripRenderer darkRenderer = (ToolStripRenderer)(Activator.CreateInstance(rendererType, nonPublic: true)
                    ?? throw new InvalidOperationException("The framework dark renderer must be constructible."));
                using Bitmap darkActual = RenderSeparator(darkRenderer, separator, backdrop);
                using Bitmap darkExpected = NewBitmap(separator.Size, backdrop);
                using (Graphics graphics = Graphics.FromImage(darkExpected))
                {
                    PaintSystemDarkSourceLines(graphics, separator, rendererType);
                }

                darkActual.Save(Path.Combine(directory, "direct-dark-renderer.png"), ImageFormat.Png);
                darkExpected.Save(Path.Combine(directory, "direct-dark-source-lines.png"), ImageFormat.Png);
                AssertPixels(darkActual, darkExpected);
                TestContext.Out.WriteLine($"availableSystemOverride={declaredOverride?.GetType().FullName} actualSourceSeparatorRoute=TOOLBARThemePart directDarkContentRect={separator.ContentRectangle}");
            }

            overBlack.Save(Path.Combine(directory, "over-black.png"), ImageFormat.Png);
            overWhite.Save(Path.Combine(directory, "over-white.png"), ImageFormat.Png);
            SaveDiagnostics(directory, actual, expected, new
            {
                inputMode = "frameworkRenderer",
                captureMethod = "rendererClientBitmap",
                dpiMode = "nativeMonitor",
                deviceDpi = strip.DeviceDpi,
                renderer = strip.Renderer.GetType().FullName,
                availableRendererOverride = declaredOverride?.GetType().FullName,
                sourcePaintRoute = professional ? "professionalSourceLines" : "TOOLBARThemePart",
                contentRectangle = new { x = separator.ContentRectangle.X, y = separator.ContentRectangle.Y, width = separator.ContentRectangle.Width, height = separator.ContentRectangle.Height },
                dark,
                enabled,
                rightToLeft,
                width,
                height,
                backdrop = Argb(backdrop),
                systemColors = ReadSystemColors(),
                nativeTheme,
                renderContext = "backdropFilledOnActualGdiDeviceContext",
                pixelsOverBlack = ReadPixels(overBlack),
                pixelsOverWhite = ReadPixels(overWhite),
                pixels = ReadPixels(actual),
            });
            AssertPixels(actual, expected);
        }
        finally
        {
            Application.SetColorMode(originalMode);
            Application.DoEvents();
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Source_professional_separator_should_not_adapt_raw_system_colors_to_custom_css(bool dark)
    {
        SystemColorMode originalMode = Application.ColorMode;
        PropertyInfo settingsProperty = typeof(ThemeFix).GetProperty(nameof(ThemeFix.ThemeSettings))
            ?? throw new MissingMemberException(typeof(ThemeFix).FullName, nameof(ThemeFix.ThemeSettings));
        ThemeSettings originalSettings = (ThemeSettings)(settingsProperty.GetValue(null)
            ?? throw new InvalidOperationException("The original ThemeFix settings must exist."));
        string directory = CreateDiagnosticDirectory();
        string themePath = Path.Combine(directory, "separator-custom.css");
        File.WriteAllText(themePath,
            $".PanelBackground {{ color: {(dark ? "#202020" : "#FFFFFF")}; }}\n"
            + ".ButtonShadow { color: #FF00FF; }\n"
            + ".Window { color: #00FF00; }\n"
            + ".ButtonHighlight { color: #0000FF; }\n");
        ThemeLoader loader = new(Substitute.For<IThemeCssUrlResolver>(), new ThemeFileReader());
        Theme theme = loader.LoadTheme(themePath, new ThemeId("separator-custom"), []);
        try
        {
            Application.SetColorMode(theme.SystemColorMode);
            Application.DoEvents();
            ThemeFix.ThemeSettings = new ThemeSettings(theme, Theme.Default, [], useSystemVisualStyle: false);
            using Form host = new() { AutoScaleMode = AutoScaleMode.None, ShowInTaskbar = false };
            using ToolStripEx strip = CreateStrip(host);
            host.FixVisualStyle();
            ToolStripProfessionalRenderer renderer = strip.Renderer.Should().BeOfType<ToolStripProfessionalRenderer>().Subject;
            theme.GetColor(KnownColor.ButtonShadow).ToArgb().Should().Be(Color.Magenta.ToArgb());
            theme.GetColor(KnownColor.Window).ToArgb().Should().Be(Color.Lime.ToArgb());
            theme.GetColor(KnownColor.ButtonHighlight).ToArgb().Should().Be(Color.Blue.ToArgb());
            AssertProfessionalColors(renderer.ColorTable);
            renderer.ColorTable.SeparatorDark.ToArgb().Should().NotBe(BlendProfessional(theme.GetColor(KnownColor.ButtonShadow), theme.GetColor(KnownColor.Window)).ToArgb());
            renderer.ColorTable.SeparatorLight.ToArgb().Should().NotBe(theme.GetColor(KnownColor.ButtonHighlight).ToArgb());
            using ToolStripSeparator separator = new() { AutoSize = false, Size = new Size(6, 25), Margin = Padding.Empty };
            strip.Items.Add(separator);
            separator.Size = new Size(6, 25);
            using Bitmap actual = RenderSeparator(strip, separator, strip.BackColor);
            using Bitmap expected = NewBitmap(separator.Size, strip.BackColor);
            using (Graphics graphics = Graphics.FromImage(expected))
            {
                PaintProfessionalSourceLines(graphics, separator, renderer.ColorTable);
            }

            SaveDiagnostics(directory, actual, expected, new
            {
                inputMode = "sourceThemeFixAndFrameworkRenderer",
                applicationColorMode = Application.ColorMode.ToString(),
                dark,
                settingsPathAccessed = false,
                customThemePath = themePath,
                customButtonShadow = Argb(theme.GetColor(KnownColor.ButtonShadow)),
                customWindow = Argb(theme.GetColor(KnownColor.Window)),
                customButtonHighlight = Argb(theme.GetColor(KnownColor.ButtonHighlight)),
                separatorDark = Argb(renderer.ColorTable.SeparatorDark),
                separatorLight = Argb(renderer.ColorTable.SeparatorLight),
                systemColors = ReadSystemColors(),
                pixels = ReadPixels(actual),
            });
            AssertPixels(actual, expected);
        }
        finally
        {
            ThemeFix.ThemeSettings = originalSettings;
            Application.SetColorMode(originalMode);
            Application.DoEvents();
        }
    }

    private static ToolStripEx CreateStrip(Form host)
    {
        ToolStripEx strip = new()
        {
            AutoSize = false,
            Dock = DockStyle.None,
            Size = new Size(160, 48),
            GripStyle = ToolStripGripStyle.Hidden,
            Padding = Padding.Empty,
            LayoutStyle = ToolStripLayoutStyle.Flow,
            BackColor = SystemColors.Control,
        };
        host.Controls.Add(strip);
        _ = host.Handle;
        _ = strip.Handle;
        return strip;
    }

    private static Bitmap RenderSeparator(ToolStrip strip, ToolStripSeparator separator, Color backdrop)
        => RenderSeparator(strip.Renderer, separator, backdrop);

    private static Bitmap RenderSeparator(ToolStripRenderer renderer, ToolStripSeparator separator, Color backdrop)
    {
        Bitmap bitmap = NewBitmap(separator.Size, backdrop);
        try
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            PaintOnInitializedDeviceContext(graphics, separator.Size, backdrop, deviceContext =>
            {
                using Graphics rendererGraphics = Graphics.FromHdc(deviceContext);
                renderer.DrawSeparator(new ToolStripSeparatorRenderEventArgs(rendererGraphics, separator, vertical: true));
            });
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static Bitmap NewBitmap(Size size, Color backdrop)
    {
        Bitmap bitmap = new(size.Width, size.Height, PixelFormat.Format24bppRgb);
        bitmap.SetResolution(96, 96);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(backdrop);
        return bitmap;
    }

    private static void PaintOnInitializedDeviceContext(Graphics graphics, Size size, Color backdrop, Action<nint> paint)
    {
        nint deviceContext = graphics.GetHdc();
        nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
        brush.Should().NotBe(0);
        try
        {
            // GDI+ exposes a temporary opaque HDC whose backdrop does not inherit
            // Graphics.Clear. Initialize the surface that UxTheme really composites.
            NativeRectangle bounds = new() { Right = size.Width, Bottom = size.Height };
            FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
            paint(deviceContext);
        }
        finally
        {
            DeleteObject(brush);
            graphics.ReleaseHdc(deviceContext);
        }
    }

    private static void PaintProfessionalSourceLines(Graphics graphics, ToolStripSeparator separator, ProfessionalColorTable colors)
    {
        // ToolstripProfessionalRenderer.RenderSeparatorInternal centers a real separator,
        // then applies its line inset. Distinct DrawLine endpoints are inclusive;
        // the source pen's flat end caps omit a zero-length line.
        Rectangle bounds = new(0, ProfessionalItemInset, separator.Width, Math.Max(0, separator.Height - ProfessionalItemTrim));
        if (bounds.Height >= ProfessionalLineInset * 2)
        {
            bounds.Inflate(0, -ProfessionalLineInset);
        }

        bool rightToLeft = separator.RightToLeft == RightToLeft.Yes;
        using Pen left = new(rightToLeft ? colors.SeparatorLight : colors.SeparatorDark);
        using Pen right = new(rightToLeft ? colors.SeparatorDark : colors.SeparatorLight);
        int x = bounds.Width / 2;
        graphics.DrawLine(left, x, bounds.Top, x, bounds.Bottom - 1);
        graphics.DrawLine(right, x + 1, bounds.Top + 1, x + 1, bounds.Bottom);
    }

    private static void AssertProfessionalColors(ProfessionalColorTable colors)
    {
        colors.SeparatorDark.ToArgb().Should().Be(BlendProfessional(SystemColors.ButtonShadow, SystemColors.Window).ToArgb());
        colors.SeparatorLight.ToArgb().Should().Be(SystemColors.ButtonHighlight.ToArgb());
    }

    private static void PaintSystemDarkSourceLines(Graphics graphics, ToolStripSeparator separator, Type rendererType)
    {
        Rectangle bounds = separator.ContentRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        MethodInfo colorMethod = rendererType.GetMethod("GetDarkModeColor", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(rendererType.FullName, "GetDarkModeColor");
        Color shadow = (Color)(colorMethod.Invoke(null, [SystemColors.ButtonShadow])
            ?? throw new InvalidOperationException("The source shadow color must exist."));
        Color controlDark = (Color)(colorMethod.Invoke(null, [SystemColors.ControlDark])
            ?? throw new InvalidOperationException("The source control-dark color must exist."));
        bool rightToLeft = separator.RightToLeft == RightToLeft.Yes;
        using Pen left = new(rightToLeft ? controlDark : shadow);
        using Pen right = new(rightToLeft ? shadow : controlDark);
        int x = bounds.Width / 2;
        graphics.DrawLine(left, x, bounds.Top, x, bounds.Bottom);
        graphics.DrawLine(right, x + 1, bounds.Top, x + 1, bounds.Bottom);
        TestContext.Out.WriteLine($"directDarkShadow={Argb(shadow)} directDarkControlDark={Argb(controlDark)} rawShadow={Argb(SystemColors.ButtonShadow)} rawControlDark={Argb(SystemColors.ControlDark)}");
    }

    private static Color BlendProfessional(Color shadow, Color window) => Color.FromArgb(
        ((70 * shadow.R) + (30 * window.R) + 50) / 100,
        ((70 * shadow.G) + (30 * window.G) + 50) / 100,
        ((70 * shadow.B) + (30 * window.B) + 50) / 100);

    private static object QueryNativeTheme(ToolStrip strip, Size size, int state)
    {
        nint theme = OpenThemeData(strip.Handle, ThemeClass);
        theme.Should().NotBe(0);
        nint deviceContext = GetDC(strip.Handle);
        deviceContext.Should().NotBe(0);
        try
        {
            NativeRectangle bounds = new() { Right = size.Width, Bottom = size.Height };
            Dictionary<string, object> properties = [];
            foreach (EnumProperty property in new[] { EnumProperty.BackgroundType, EnumProperty.SizingType, EnumProperty.BorderType, EnumProperty.FillType, EnumProperty.ImageLayout })
            {
                int result = GetThemeEnumValue(theme, SeparatorPart, state, (int)property, out int value);
                properties[property.ToString()] = new { hresult = result, value };
            }

            foreach (IntegerProperty property in new[] { IntegerProperty.BorderSize, IntegerProperty.ImageCount, IntegerProperty.GradientRatio1, IntegerProperty.GradientRatio2, IntegerProperty.GradientRatio3, IntegerProperty.GradientRatio4, IntegerProperty.GradientRatio5 })
            {
                int result = GetThemeInt(theme, SeparatorPart, state, (int)property, out int value);
                properties[property.ToString()] = new { hresult = result, value };
            }

            foreach (ColorProperty property in new[] { ColorProperty.BorderColor, ColorProperty.FillColor, ColorProperty.GradientColor1, ColorProperty.GradientColor2, ColorProperty.GradientColor3, ColorProperty.GradientColor4, ColorProperty.GradientColor5 })
            {
                int result = GetThemeColor(theme, SeparatorPart, state, (int)property, out uint value);
                properties[property.ToString()] = new { hresult = result, argb = result >= 0 ? Argb(Color.FromArgb((int)(value & 255), (int)((value >> 8) & 255), (int)((value >> 16) & 255))) : null };
            }

            foreach (MarginProperty property in new[] { MarginProperty.SizingMargins, MarginProperty.ContentMargins })
            {
                int result = GetThemeMargins(theme, deviceContext, SeparatorPart, state, (int)property, ref bounds, out NativeMargins margins);
                properties[property.ToString()] = new { hresult = result, left = margins.Left, right = margins.Right, top = margins.Top, bottom = margins.Bottom };
            }

            for (int sizing = 0; sizing <= 2; sizing++)
            {
                int result = GetThemePartSize(theme, deviceContext, SeparatorPart, state, ref bounds, sizing, out NativeSize partSize);
                properties[$"PartSize{(ThemeSizeType)sizing}"] = new { hresult = result, width = partSize.Width, height = partSize.Height };
            }

            int contentResult = GetThemeBackgroundContentRect(theme, deviceContext, SeparatorPart, state, ref bounds, out NativeRectangle content);
            properties["BackgroundContentRect"] = new { hresult = contentResult, left = content.Left, top = content.Top, right = content.Right, bottom = content.Bottom };
            object image = ReadNativeThemeBitmap(theme, deviceContext, state);
            return new { themeClass = ThemeClass, part = SeparatorPart, state, queryContext = "initializedToolStripWindowHdc", properties, image };
        }
        finally
        {
            ReleaseDC(strip.Handle, deviceContext);
            CloseThemeData(theme);
        }
    }

    private static object ReadNativeThemeBitmap(nint theme, nint deviceContext, int state)
    {
        // Property zero selects the native part image; GBF_COPY gives this fixture
        // ownership without changing or deleting the theme service's cached bitmap.
        int result = GetThemeBitmap(theme, SeparatorPart, state, 0, 2, out nint bitmap);
        if (result < 0 || bitmap == 0)
        {
            return new { query = "GetThemeBitmap(property0,GBF_COPY)", hresult = result, available = false };
        }

        try
        {
            GetObject(bitmap, Marshal.SizeOf<NativeBitmap>(), out NativeBitmap description).Should().Be(Marshal.SizeOf<NativeBitmap>());
            NativeBitmapInfo information = new()
            {
                HeaderSize = (uint)Marshal.SizeOf<NativeBitmapInfo>(),
                Width = description.Width,
                Height = -description.Height,
                Planes = 1,
                BitCount = 32,
            };
            byte[] bytes = new byte[description.Width * description.Height * 4];
            GetDIBits(deviceContext, bitmap, 0, (uint)description.Height, bytes, ref information, 0).Should().Be(description.Height);
            string[][] pixels = Enumerable.Range(0, description.Height).Select(y => Enumerable.Range(0, description.Width).Select(x =>
            {
                int offset = ((y * description.Width) + x) * 4;
                return $"#{bytes[offset + 3]:X2}{bytes[offset + 2]:X2}{bytes[offset + 1]:X2}{bytes[offset]:X2}";
            }).ToArray()).ToArray();
            return new
            {
                query = "GetThemeBitmap(property0,GBF_COPY)",
                hresult = result,
                available = true,
                width = description.Width,
                height = description.Height,
                nativeBitsPerPixel = description.BitsPerPixel,
                pixelEncoding = "rawNative32BitBiRgbBgraShownAsArgb",
                pixels,
            };
        }
        finally
        {
            DeleteObject(bitmap);
        }
    }

    private static object ReadSystemColors() => new
    {
        control = Argb(SystemColors.Control),
        buttonShadow = Argb(SystemColors.ButtonShadow),
        window = Argb(SystemColors.Window),
        buttonHighlight = Argb(SystemColors.ButtonHighlight),
    };

    private static string[][] ReadPixels(Bitmap bitmap) => Enumerable.Range(0, bitmap.Height)
        .Select(y => Enumerable.Range(0, bitmap.Width).Select(x => Argb(bitmap.GetPixel(x, y))).ToArray()).ToArray();

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    private static string CreateDiagnosticDirectory()
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "SeparatorProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void SaveDiagnostics(string directory, Bitmap actual, Bitmap expected, object metadata)
    {
        actual.Save(Path.Combine(directory, "actual.png"), ImageFormat.Png);
        expected.Save(Path.Combine(directory, "expected.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "probe.json"), JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        TestContext.Out.WriteLine($"separatorProbeEvidence={directory}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(metadata));
    }

    private static void AssertPixels(Bitmap actual, Bitmap expected)
    {
        for (int y = 0; y < actual.Height; y++)
        {
            for (int x = 0; x < actual.Width; x++)
            {
                actual.GetPixel(x, y).ToArgb().Should().Be(expected.GetPixel(x, y).ToArgb(), $"the real renderer must paint its source/theme contract at ({x},{y})");
            }
        }
    }

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern nint OpenThemeData(nint window, string className);

    [DllImport("uxtheme.dll")]
    private static extern int CloseThemeData(nint theme);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeEnumValue(nint theme, int part, int state, int property, out int value);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeInt(nint theme, int part, int state, int property, out int value);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeColor(nint theme, int part, int state, int property, out uint value);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeMargins(nint theme, nint deviceContext, int part, int state, int property, ref NativeRectangle bounds, out NativeMargins margins);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemePartSize(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, int sizing, out NativeSize size);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeBackgroundContentRect(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, out NativeRectangle content);

    [DllImport("uxtheme.dll")]
    private static extern int DrawThemeBackground(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, nint clippingBounds);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeBitmap(nint theme, int part, int state, int property, uint flags, out nint bitmap);

    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")]
    private static extern int GetObject(nint handle, int bufferSize, out NativeBitmap bitmap);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(nint deviceContext, nint bitmap, uint firstScanLine, uint scanLines, byte[] pixels, ref NativeBitmapInfo information, uint usage);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint handle);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint deviceContext, ref NativeRectangle bounds, nint brush);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMargins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBitmap
    {
        public int Type;
        public int Width;
        public int Height;
        public int WidthBytes;
        public ushort Planes;
        public ushort BitsPerPixel;
        public nint Bits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBitmapInfo
    {
        public uint HeaderSize;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint ImageSize;
        public int PixelsPerMeterX;
        public int PixelsPerMeterY;
        public uint ColorsUsed;
        public uint ColorsImportant;
    }
}
