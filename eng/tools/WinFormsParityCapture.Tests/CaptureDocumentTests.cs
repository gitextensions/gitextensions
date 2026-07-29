using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class CaptureDocumentTests
{
    [Test]
    public void Serialize_should_round_trip_byte_identically()
    {
        CaptureDocument expected = CreateDocument("#FF010203");

        string first = CaptureJson.Serialize(expected);
        CaptureDocument actual = CaptureJson.Deserialize(first);
        string second = CaptureJson.Serialize(actual);

        second.Should().Be(first);
        first.Should().NotContain("\"file\"");
        first.ToLowerInvariant().Should().NotContain("createdat");
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Serialize_should_reject_symbolic_colors()
    {
        CaptureDocument document = CreateDocument("SystemColors.Control");

        Action action = () => CaptureJson.Serialize(document);

        action.Should().Throw<InvalidDataException>()
            .WithMessage("*#AARRGGBB*");
    }

    [Test]
    public void FormatArgb_should_emit_uppercase_concrete_value()
    {
        string actual = CaptureJson.FormatArgb(0xFF, 0x0A, 0xBC, 0x01);

        actual.Should().Be("#FF0ABC01");
    }

    [Test]
    public void TreeSchema_should_define_a_closed_versioned_contract()
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "tree.schema.json");
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllText(path));

        schema.RootElement.GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        JsonElement definitions = schema.RootElement.GetProperty("$defs");
        definitions.GetProperty("node").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("column").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("colors").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("capture").GetProperty("properties").GetProperty("dpiMode")
            .GetProperty("enum").GetArrayLength().Should().Be(2);
        definitions.GetProperty("image").GetProperty("properties").TryGetProperty("file", out _).Should().BeFalse();
    }

    private static CaptureDocument CreateDocument(string foreground) =>
        new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            Component = new CaptureComponent { TypeName = "Tests.Form", AssemblyName = "Tests" },
            Capture = new CaptureMetadata
            {
                Framework = "winforms",
                Theme = new CaptureTheme
                {
                    Id = "light",
                    Kind = "builtin",
                    SourceSha256 = new string('A', 64)
                },
                ScalePercent = 100,
                Dpi = new CaptureDpi { X = 96, Y = 96 },
                DpiMode = CaptureDpiMode.NativeMonitor,
                State = "normal",
                StateStatus = CaptureStateStatus.Captured
            },
            Image = new CaptureImage
            {
                WidthPx = 1,
                HeightPx = 1,
                CaptureMethod = CaptureMethod.PrintWindow
            },
            Surfaces =
            [
                new CaptureSurface
                {
                    Role = "primary",
                    ScreenBoundsPx = new CaptureRectangle { X = 0, Y = 0, Width = 1, Height = 1 },
                    Root = new CaptureNode
                    {
                        Id = "root",
                        FieldAliases = [],
                        Type = "Tests.Form",
                        ControlKind = "window",
                        BoundsPx = new CaptureRectangle { X = 0, Y = 0, Width = 1, Height = 1 },
                        BoundsDip = new CaptureRectangleF { X = 0, Y = 0, Width = 1, Height = 1 },
                        ClientSizePx = new CaptureSize { Width = 1, Height = 1 },
                        ClientSizeDip = new CaptureSizeF { Width = 1, Height = 1 },
                        Padding = CreateThickness(),
                        Margin = CreateThickness(),
                        Colors = new CaptureColors
                        {
                            Foreground = foreground,
                            Additional = new Dictionary<string, string>()
                        },
                        Anchor = [],
                        Columns = [],
                        Children = []
                    }
                }
            ]
        };

    private static CaptureThicknessPair CreateThickness() =>
        new()
        {
            Px = new CaptureThickness { Left = 0, Top = 0, Right = 0, Bottom = 0 },
            Dip = new CaptureThicknessF { Left = 0, Top = 0, Right = 0, Bottom = 0 }
        };
}
