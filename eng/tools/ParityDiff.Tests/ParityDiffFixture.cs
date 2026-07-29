using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitExtensions.ParityCapture;

namespace GitExtensions.ParityDiff.Tests;

// parity-scaffolding: Builds isolated inputs for temporary comparison-tool tests.
internal sealed class ParityDiffFixture : IDisposable
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = CreateManifestJsonOptions();
    private readonly string _rootDirectory = Path.Combine(
        Path.GetTempPath(),
        $"GitExtensions.ParityDiff.Tests-{Guid.NewGuid():N}");

    public ParityDiffFixture()
    {
        Directory.CreateDirectory(_rootDirectory);
        ConfigurationFile = Path.Combine(_rootDirectory, "parity-diff.json");
        File.WriteAllText(
            ConfigurationFile,
            """
            {
              "schemaVersion": 1,
              "defaults": {
                "geometryDip": 0.5,
                "fontSizePoints": 0.01,
                "borderWidthDip": 0.01,
                "cornerRadiusDip": 0.01,
                "pixels": {
                  "minimumSsim": 1.0,
                  "maximumDifferentPixelFraction": 0.0,
                  "maximumChannelDelta": 0
                }
              }
            }
            """);
    }

    public string ConfigurationFile { get; }

    public string OutputDirectory => Path.Combine(_rootDirectory, "output");

    public CaptureDocument CreateDocument(string themeId) =>
        new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            Component = new CaptureComponent
            {
                TypeName = "Tests.Form",
                AssemblyName = "Tests"
            },
            Capture = new CaptureMetadata
            {
                Framework = "test",
                Theme = new CaptureTheme
                {
                    Id = themeId,
                    Kind = "builtin",
                    SourceSha256 = new string('A', 64)
                },
                ScalePercent = 100,
                Dpi = new CaptureDpi { X = 96, Y = 96 },
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                State = "normal",
                StateStatus = CaptureStateStatus.Captured
            },
            Image = new CaptureImage
            {
                WidthPx = 1,
                HeightPx = 1,
                CaptureMethod = CaptureMethod.HeadlessSkia
            },
            Surfaces =
            [
                new CaptureSurface
                {
                    Role = "primary",
                    ScreenBoundsPx = new CaptureRectangle { X = 0, Y = 0, Width = 1, Height = 1 },
                    Root = CreateNode(
                        id: "root",
                        fieldName: null,
                        controlKind: "window",
                        children:
                        [
                            CreateNode(
                                id: "root/btnTarget",
                                fieldName: "btnTarget",
                                controlKind: "button",
                                children: [])
                        ])
                }
            ]
        };

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }
    }

    public ParityDiffResult Run() =>
        ParityDiffRunner.Run(new DiffOptions
        {
            ReferenceManifest = Path.Combine(_rootDirectory, "reference", "manifest.json"),
            CandidateManifest = Path.Combine(_rootDirectory, "candidate", "manifest.json"),
            ConfigurationFile = ConfigurationFile,
            OutputDirectory = OutputDirectory
        });

    public void WriteCaptureSet(string name, IReadOnlyList<CaptureDocument> documents, byte red = 32)
    {
        string directory = Path.Combine(_rootDirectory, name);
        Directory.CreateDirectory(directory);
        List<CaptureManifestEntry> entries = [];
        foreach (CaptureDocument document in documents)
        {
            string stem = $"{document.Capture.Theme.Id}-{document.Capture.State}";
            string treeFile = $"{stem}.tree.json";
            string imageFile = $"{stem}.png";
            File.WriteAllText(Path.Combine(directory, treeFile), CaptureJson.Serialize(document));
            WritePng(Path.Combine(directory, imageFile), red, green: 64, blue: 96, alpha: 255);
            entries.Add(new CaptureManifestEntry
            {
                ComponentType = document.Component.TypeName,
                ThemeId = document.Capture.Theme.Id,
                ScalePercent = document.Capture.ScalePercent,
                State = document.Capture.State,
                Status = CaptureStateStatus.Captured,
                DpiMode = document.Capture.DpiMode,
                CaptureMethod = document.Image.CaptureMethod,
                ImageFile = imageFile,
                TreeFile = treeFile
            });
        }

        WriteManifest(directory, entries);
    }

    public void WriteUnsupportedCaptureSet(string name, string note)
    {
        string directory = Path.Combine(_rootDirectory, name);
        Directory.CreateDirectory(directory);
        WriteManifest(
            directory,
            [
                new CaptureManifestEntry
                {
                    ComponentType = "Tests.Form",
                    ThemeId = "light",
                    ScalePercent = 100,
                    State = "normal",
                    Status = CaptureStateStatus.Unsupported,
                    Note = note,
                    CaptureMethod = CaptureMethod.Unsupported
                }
            ]);
    }

    private static void WriteManifest(string directory, IReadOnlyList<CaptureManifestEntry> entries)
    {
        CaptureSetManifest manifest = new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            CreatedAtUtc = DateTime.UnixEpoch,
            ToolVersion = "1.0.0",
            Repository = "fixture",
            Captures = entries
        };
        File.WriteAllText(
            Path.Combine(directory, "manifest.json"),
            JsonSerializer.Serialize(manifest, ManifestJsonOptions));
    }

    private static JsonSerializerOptions CreateManifestJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static CaptureNode CreateNode(
        string id,
        string? fieldName,
        string controlKind,
        IReadOnlyList<CaptureNode> children) =>
        new()
        {
            Id = id,
            FieldName = fieldName,
            FieldAliases = [],
            Name = fieldName,
            Type = $"Tests.{controlKind}",
            ControlKind = controlKind,
            BoundsPx = new CaptureRectangle { X = 0, Y = 0, Width = 10, Height = 10 },
            BoundsDip = new CaptureRectangleF { X = 0, Y = 0, Width = 10, Height = 10 },
            ClientSizePx = new CaptureSize { Width = 10, Height = 10 },
            ClientSizeDip = new CaptureSizeF { Width = 10, Height = 10 },
            Padding = CreateThickness(),
            Margin = CreateThickness(),
            Font = new CaptureFont
            {
                Family = "Test Sans",
                EmSize = 9,
                Unit = "point",
                SizePoints = 9,
                SizeDip = 12,
                Style = ["regular"]
            },
            Colors = new CaptureColors
            {
                Foreground = "#FF010203",
                Background = "#FF040506",
                Additional = new Dictionary<string, string>()
            },
            BorderStyle = "none",
            BorderWidthDip = 0,
            CornerRadiusDip = new CaptureCornerRadius
            {
                TopLeft = 0,
                TopRight = 0,
                BottomRight = 0,
                BottomLeft = 0
            },
            Anchor = [],
            AutoSize = false,
            Text = fieldName,
            TabIndex = fieldName is null ? null : 0,
            TabStop = fieldName is not null,
            Enabled = true,
            Visible = true,
            Focused = false,
            Columns = [],
            Children = children
        };

    private static CaptureThicknessPair CreateThickness() =>
        new()
        {
            Px = new CaptureThickness { Left = 0, Top = 0, Right = 0, Bottom = 0 },
            Dip = new CaptureThicknessF { Left = 0, Top = 0, Right = 0, Bottom = 0 }
        };

    private static void WriteChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        stream.Write(Encoding.ASCII.GetBytes(type));
        stream.Write(data);
        stream.Write([0, 0, 0, 0]);
    }

    private static void WritePng(string path, byte red, byte green, byte blue, byte alpha)
    {
        using FileStream stream = File.Create(path);
        stream.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header[..4], 1);
        BinaryPrimitives.WriteInt32BigEndian(header.Slice(4, 4), 1);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(stream, "IHDR", header);

        using MemoryStream compressed = new();
        using (ZLibStream zlib = new(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write([0, red, green, blue, alpha]);
        }

        WriteChunk(stream, "IDAT", compressed.ToArray());
        WriteChunk(stream, "IEND", []);
    }
}
