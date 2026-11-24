using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class CaptureDocumentTests
{
    private static readonly DateTime FixtureClockUtc = new DateTime(2026, 10, 24, 12, 34, 56, DateTimeKind.Utc).AddTicks(1234567);
    private static readonly JsonSerializerOptions MatrixJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Test]
    public void Serialize_should_round_trip_byte_identically()
    {
        CaptureDocument expected = CreateDocument("#FF010203");

        string first = CaptureJson.Serialize(expected);
        CaptureDocument actual = CaptureJson.Deserialize(first);
        string second = CaptureJson.Serialize(actual);

        second.Should().Be(first);
        first.Should().NotContain("\"file\"");
        first.Should().NotContain("\"acquisitions\"");
        first.Should().NotContain("fixtureNowUtc");
        first.ToLowerInvariant().Should().NotContain("createdat");
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Serialize_should_use_canonical_CRLF_and_canonicalize_legacy_LF_without_changing_values()
    {
        CaptureDocument expected = CreateDocument("#FF010203");
        CaptureSurface surface = expected.Surfaces[0];
        expected = expected with
        {
            Surfaces = [surface with { Root = surface.Root with { Text = "first\nsecond\r\nthird\rfourth" } }]
        };

        string canonical = CaptureJson.Serialize(expected);

        canonical.Should().StartWith("{\r\n").And.EndWith("}\r\n");
        canonical.Replace("\r\n", string.Empty, StringComparison.Ordinal).Should().NotContain("\r").And.NotContain("\n");
        string legacy = canonical.Replace("\r\n", "\n", StringComparison.Ordinal);
        legacy.Should().NotContain("\r").And.EndWith("}\n");
        CaptureDocument actual = CaptureJson.Deserialize(legacy);
        actual.Should().BeEquivalentTo(expected);
        CaptureJson.Serialize(actual).Should().Be(canonical);
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
    public void Serialize_should_omit_empty_optional_acquisitions_without_changing_existing_documents()
    {
        CaptureDocument document = CreateDocument("#FF010203");
        CaptureDocument empty = document with { Image = document.Image with { Acquisitions = [] } };

        CaptureJson.Serialize(empty).Should().Be(CaptureJson.Serialize(document));
    }

    [Test]
    public void Serialize_should_round_trip_composite_acquisition_provenance_byte_identically()
    {
        CaptureDocument document = CreateCompositeDocument();

        string first = CaptureJson.Serialize(document);
        CaptureDocument actual = CaptureJson.Deserialize(first);

        CaptureJson.Serialize(actual).Should().Be(first);
        actual.Should().BeEquivalentTo(document);
        first.Should().Contain("\"captureMethod\": \"printWindowScreenGrabComposite\"");
        first.Should().Contain("\"reason\": \"redrawDisabledNativeSurface\"");
        first.Should().NotContain("handle");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Serialize_should_reject_composite_without_acquisition_regions(bool empty)
    {
        CaptureDocument document = CreateCompositeDocument();
        document = document with { Image = document.Image with { Acquisitions = empty ? [] : null } };

        Action serialize = () => CaptureJson.Serialize(document);

        serialize.Should().Throw<InvalidDataException>().WithMessage("*must record its acquired regions*");
    }

    [TestCase("surface")]
    [TestCase("method")]
    [TestCase("reason")]
    [TestCase("negative")]
    [TestCase("empty")]
    [TestCase("outside")]
    [TestCase("overflow")]
    [TestCase("overallMethod")]
    public void Serialize_should_reject_inconsistent_or_out_of_bounds_acquisition_provenance(string invalid)
    {
        CaptureDocument document = CreateCompositeDocument();
        CaptureImageAcquisition acquisition = document.Image.Acquisitions?.Single()
            ?? throw new InvalidOperationException("The composite fixture requires an acquisition.");
        acquisition = invalid switch
        {
            "surface" => acquisition with { SurfaceRole = "popup0" },
            "method" => acquisition with { CaptureMethod = CaptureMethod.PrintWindow },
            "reason" => acquisition with { Reason = "blankPixels" },
            "negative" => acquisition with { RegionPx = acquisition.RegionPx with { X = -1 } },
            "empty" => acquisition with { RegionPx = acquisition.RegionPx with { Width = 0 } },
            "outside" => acquisition with { RegionPx = acquisition.RegionPx with { Width = 2 } },
            "overflow" => acquisition with { RegionPx = acquisition.RegionPx with { X = int.MaxValue, Width = int.MaxValue } },
            _ => acquisition
        };
        document = document with
        {
            Image = document.Image with
            {
                CaptureMethod = invalid == "overallMethod" ? CaptureMethod.PrintWindow : document.Image.CaptureMethod,
                Acquisitions = [acquisition]
            }
        };

        Action serialize = () => CaptureJson.Serialize(document);

        serialize.Should().Throw<InvalidDataException>();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CapturePlan_should_omit_absent_fixture_clock_and_load_legacy_plans(bool explicitNull)
    {
        CapturePlan plan = CreatePlan();
        string legacy = JsonSerializer.Serialize(new
        {
            plan.SchemaVersion,
            plan.SettingsProfile,
            plan.Scales,
            plan.Themes,
            plan.Components
        }, MatrixJsonOptions);
        string json = explicitNull ? SerializeWithFixtureClock(plan, "null") : legacy;

        CapturePlan actual = LoadPlan(json);

        plan.FixtureNowUtc.Should().BeNull();
        actual.FixtureNowUtc.Should().BeNull();
        JsonSerializer.Serialize(plan, MatrixJsonOptions).Should().Be(legacy);
        JsonSerializer.Serialize(actual, MatrixJsonOptions).Should().Be(legacy);
    }

    [Test]
    public void CapturePlan_should_load_a_utc_fixture_clock_with_exact_ticks_and_kind()
    {
        CapturePlan plan = CreatePlan() with { FixtureNowUtc = FixtureClockUtc };
        string json = JsonSerializer.Serialize(plan, MatrixJsonOptions);

        CapturePlan actual = LoadPlan(json);

        DateTime clock = actual.FixtureNowUtc ?? throw new InvalidOperationException("The plan fixture clock must round trip.");
        clock.Ticks.Should().Be(FixtureClockUtc.Ticks);
        clock.Kind.Should().Be(DateTimeKind.Utc);
        json.Should().Contain("\"fixtureNowUtc\": \"2026-10-24T12:34:56.1234567Z\"");
        JsonSerializer.Serialize(actual, MatrixJsonOptions).Should().Be(json);
    }

    [TestCase("2026-10-24T12:34:56.1234567")]
    [TestCase("2026-10-24T12:34:56.1234567+00:00")]
    [TestCase("2026-10-24T12:34:56.1234567+02:00")]
    [TestCase("2026-10-24T12:34:56.1234567-03:00")]
    public void CapturePlan_Load_should_reject_non_utc_fixture_clock_json(string timestamp)
    {
        string json = SerializeWithFixtureClock(CreatePlan(), JsonSerializer.Serialize(timestamp));

        Action load = () => LoadPlan(json);

        load.Should().Throw<InvalidDataException>().WithMessage("*UTC*");
    }

    [TestCase("\"not-a-date\"")]
    [TestCase("\"2026-13-24T12:34:56Z\"")]
    [TestCase("42")]
    public void CapturePlan_Load_should_reject_malformed_fixture_clock_json(string clockJson)
    {
        string json = SerializeWithFixtureClock(CreatePlan(), clockJson);

        Action load = () => LoadPlan(json);

        load.Should().Throw<JsonException>();
    }

    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Unspecified)]
    public void CapturePlan_should_reject_non_utc_fixture_clock_initializers(DateTimeKind kind)
    {
        DateTime clock = DateTime.SpecifyKind(FixtureClockUtc, kind);

        Action create = () => _ = CreatePlan() with { FixtureNowUtc = clock };

        create.Should().Throw<InvalidDataException>().WithMessage("*UTC*");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CaptureSetManifest_should_omit_absent_fixture_clock_and_read_legacy_manifests(bool explicitNull)
    {
        CaptureSetManifest manifest = CreateManifest();
        string legacy = JsonSerializer.Serialize(new
        {
            manifest.SchemaVersion,
            manifest.CreatedAtUtc,
            manifest.ToolVersion,
            manifest.Repository,
            manifest.Captures
        }, MatrixJsonOptions);
        string json = explicitNull ? SerializeWithFixtureClock(manifest, "null") : legacy;

        CaptureSetManifest actual = JsonSerializer.Deserialize<CaptureSetManifest>(json, MatrixJsonOptions)
            ?? throw new InvalidOperationException("The legacy manifest must deserialize.");

        manifest.FixtureNowUtc.Should().BeNull();
        actual.FixtureNowUtc.Should().BeNull();
        JsonSerializer.Serialize(manifest, MatrixJsonOptions).Should().Be(legacy);
        JsonSerializer.Serialize(actual, MatrixJsonOptions).Should().Be(legacy);
        actual.CreatedAtUtc.Ticks.Should().Be(manifest.CreatedAtUtc.Ticks);
        actual.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public void CaptureSetManifest_should_round_trip_a_utc_fixture_clock_without_changing_acquisition_time()
    {
        CaptureSetManifest manifest = CreateManifest() with { FixtureNowUtc = FixtureClockUtc };
        string json = JsonSerializer.Serialize(manifest, MatrixJsonOptions);

        CaptureSetManifest actual = JsonSerializer.Deserialize<CaptureSetManifest>(json, MatrixJsonOptions)
            ?? throw new InvalidOperationException("The fixture-clock manifest must deserialize.");

        DateTime clock = actual.FixtureNowUtc ?? throw new InvalidOperationException("The manifest fixture clock must round trip.");
        clock.Ticks.Should().Be(FixtureClockUtc.Ticks);
        clock.Kind.Should().Be(DateTimeKind.Utc);
        actual.CreatedAtUtc.Ticks.Should().Be(manifest.CreatedAtUtc.Ticks);
        actual.CreatedAtUtc.Ticks.Should().NotBe(clock.Ticks);
        actual.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        json.Should().Contain("\"fixtureNowUtc\": \"2026-10-24T12:34:56.1234567Z\"");
        JsonSerializer.Serialize(actual, MatrixJsonOptions).Should().Be(json);
    }

    [TestCase("2026-10-24T12:34:56.1234567")]
    [TestCase("2026-10-24T12:34:56.1234567+00:00")]
    [TestCase("2026-10-24T12:34:56.1234567+02:00")]
    [TestCase("2026-10-24T12:34:56.1234567-03:00")]
    public void CaptureSetManifest_should_reject_non_utc_fixture_clock_json(string timestamp)
    {
        string json = SerializeWithFixtureClock(CreateManifest(), JsonSerializer.Serialize(timestamp));

        Action deserialize = () => JsonSerializer.Deserialize<CaptureSetManifest>(json, MatrixJsonOptions);

        deserialize.Should().Throw<InvalidDataException>().WithMessage("*UTC*");
    }

    [TestCase("\"not-a-date\"")]
    [TestCase("\"2026-13-24T12:34:56Z\"")]
    [TestCase("42")]
    public void CaptureSetManifest_should_reject_malformed_fixture_clock_json(string clockJson)
    {
        string json = SerializeWithFixtureClock(CreateManifest(), clockJson);

        Action deserialize = () => JsonSerializer.Deserialize<CaptureSetManifest>(json, MatrixJsonOptions);

        deserialize.Should().Throw<JsonException>();
    }

    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Unspecified)]
    public void CaptureSetManifest_should_reject_non_utc_fixture_clock_initializers(DateTimeKind kind)
    {
        DateTime clock = DateTime.SpecifyKind(FixtureClockUtc, kind);

        Action create = () => _ = CreateManifest() with { FixtureNowUtc = clock };

        create.Should().Throw<InvalidDataException>().WithMessage("*UTC*");
    }

    [Test]
    public void TreeSchema_should_define_a_closed_versioned_contract()
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "tree.schema.json");
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllText(path));

        schema.RootElement.GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        schema.RootElement.GetProperty("properties").GetProperty("schemaVersion")
            .GetProperty("const").GetInt32().Should().Be(CaptureDocument.CurrentSchemaVersion);
        JsonElement definitions = schema.RootElement.GetProperty("$defs");
        definitions.GetProperty("node").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("column").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("colors").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("capture").GetProperty("properties").GetProperty("dpiMode")
            .GetProperty("enum").EnumerateArray().Select(value => value.GetString()).Should()
            .Equal("nativeMonitor", "dpiChangeMessage", "headlessRenderScale");
        definitions.GetProperty("image").GetProperty("properties").GetProperty("captureMethod")
            .GetProperty("enum").EnumerateArray().Select(value => value.GetString()).Should()
            .BeEquivalentTo(Enum.GetValues<CaptureMethod>().Where(method => method != CaptureMethod.Unsupported)
                .Select(method => JsonNamingPolicy.CamelCase.ConvertName(method.ToString())));
        definitions.GetProperty("imageAcquisition").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        definitions.GetProperty("imageAcquisition").GetProperty("properties").GetProperty("surfaceRole")
            .GetProperty("const").GetString().Should().Be("primary");
        JsonElement acquisitionRegion = definitions.GetProperty("imageAcquisition").GetProperty("properties")
            .GetProperty("regionPx").GetProperty("properties");
        acquisitionRegion.GetProperty("x").GetProperty("minimum").GetInt32().Should().Be(0);
        acquisitionRegion.GetProperty("y").GetProperty("minimum").GetInt32().Should().Be(0);
        acquisitionRegion.GetProperty("width").GetProperty("minimum").GetInt32().Should().Be(1);
        acquisitionRegion.GetProperty("height").GetProperty("minimum").GetInt32().Should().Be(1);
        definitions.GetProperty("image").GetProperty("then").GetProperty("required")
            .EnumerateArray().Select(value => value.GetString()).Should().Contain("acquisitions");
        definitions.GetProperty("image").GetProperty("properties").TryGetProperty("file", out _).Should().BeFalse();
        definitions.GetProperty("node").GetProperty("properties").GetProperty("itemHeightDip")
            .GetProperty("$ref").GetString().Should().Be("#/$defs/nullableNumber");
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

    private static CaptureDocument CreateCompositeDocument()
    {
        CaptureDocument document = CreateDocument("#FF010203");
        return document with
        {
            Image = document.Image with
            {
                CaptureMethod = CaptureMethod.PrintWindowScreenGrabComposite,
                Acquisitions =
                [
                    new CaptureImageAcquisition
                    {
                        SurfaceRole = "primary",
                        RegionPx = new CaptureRectangle { X = 0, Y = 0, Width = 1, Height = 1 },
                        CaptureMethod = CaptureMethod.ScreenGrab,
                        Reason = "redrawDisabledNativeSurface"
                    }
                ]
            }
        };
    }

    private static CaptureSetManifest CreateManifest() =>
        new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            CreatedAtUtc = new DateTime(2026, 10, 24, 18, 0, 0, DateTimeKind.Utc),
            ToolVersion = "Tests",
            Repository = "fixture",
            Captures = []
        };

    private static CapturePlan CreatePlan() =>
        new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            SettingsProfile = "fixture",
            Scales = [100],
            Themes = [new CaptureThemePlan { Id = "light", Kind = "builtin", File = "light.css", IsBuiltin = true }],
            Components =
            [
                new CaptureComponentPlan
                {
                    TypeName = "Tests.Form",
                    States = [new CaptureStatePlan { Id = "normal", Kind = CaptureStateKind.Normal }]
                }
            ]
        };

    private static CaptureThicknessPair CreateThickness() =>
        new()
        {
            Px = new CaptureThickness { Left = 0, Top = 0, Right = 0, Bottom = 0 },
            Dip = new CaptureThicknessF { Left = 0, Top = 0, Right = 0, Bottom = 0 }
        };

    private static CapturePlan LoadPlan(string json)
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            return CapturePlan.Load(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string SerializeWithFixtureClock<T>(T matrix, string clockJson) =>
        JsonSerializer.Serialize(matrix, MatrixJsonOptions).Insert(1, $"\"fixtureNowUtc\": {clockJson},");
}
