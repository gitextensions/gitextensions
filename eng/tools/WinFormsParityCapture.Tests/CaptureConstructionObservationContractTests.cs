using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// parity-scaffolding: These cases validate measurement metadata without constructing a UI or replacing its clock.

/// <summary>
///  Verifies the framework-neutral, manifest-only factory observation contract on both capture consumers.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CaptureConstructionObservationContractTests
{
    private static readonly DateTimeOffset ObservedInstant = new DateTimeOffset(2026, 11, 3, 12, 34, 56, TimeSpan.Zero).AddMilliseconds(100);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [TestCase("")]
    [TestCase("en-US")]
    [TestCase("de-DE")]
    [TestCase("ar-SA")]
    [TestCase("sr-Latn-RS")]
    public void Sample_should_derive_UTC_local_and_general_date_text_from_one_observation(string cultureName)
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUICulture = CultureInfo.CurrentUICulture;
        TimeZoneInfo originalZone = TimeZoneInfo.Local;
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
        TimeZoneInfo zone = TimeZoneInfo.CreateCustomTimeZone("Fixture/Local", TimeSpan.FromHours(2), "Fixture local", "Fixture standard");
        CaptureConstructionFont font = CreateFont();

        CaptureConstructionClockSample sample = CaptureConstructionClock.Sample(
            ObservedInstant, culture, CultureInfo.InvariantCulture, zone, font, relativeDate: false);

        sample.ObservedAtUtc.Should().Be(ObservedInstant.UtcDateTime);
        sample.ObservedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        sample.ObservedAtLocal.UtcDateTime.Should().Be(sample.ObservedAtUtc);
        sample.ObservedAtLocal.Offset.Should().Be(TimeSpan.FromHours(2));
        sample.GeneralDateTimeText.Should().Be(ObservedInstant.ToOffset(TimeSpan.FromHours(2)).DateTime.ToString("G", culture));
        sample.Context.CultureName.Should().Be(culture.Name);
        sample.Context.UICultureName.Should().BeEmpty();
        sample.Context.ShortDatePattern.Should().Be(culture.DateTimeFormat.ShortDatePattern);
        sample.Context.LongTimePattern.Should().Be(culture.DateTimeFormat.LongTimePattern);
        sample.Context.CalendarType.Should().Be(culture.DateTimeFormat.Calendar.GetType().FullName);
        sample.Context.TimeZoneId.Should().Be(zone.Id);
        sample.Context.ConfiguredFont.Should().Be(font);
        CultureInfo.CurrentCulture.Should().BeSameAs(originalCulture);
        CultureInfo.CurrentUICulture.Should().BeSameAs(originalUICulture);
        TimeZoneInfo.Local.Should().BeSameAs(originalZone);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void Sample_should_retain_valid_empty_or_whitespace_culture_overrides(string separator)
    {
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.DateTimeFormat.DateSeparator = separator;
        culture.DateTimeFormat.TimeSeparator = separator;
        culture.DateTimeFormat.AMDesignator = separator;
        culture.DateTimeFormat.PMDesignator = separator;
        CaptureConstructionClockSample sample = CaptureConstructionClock.Sample(
            ObservedInstant, culture, CultureInfo.InvariantCulture, TimeZoneInfo.Utc, CreateFont(), relativeDate: false);

        CaptureConstructionObservation observation = CaptureConstructionClock.Create(sample, sample, [], []);
        CaptureManifestEntry entry = CreateEntry(observation);
        CaptureManifestEntry actual = JsonSerializer.Deserialize<CaptureManifestEntry>(JsonSerializer.Serialize(entry, JsonOptions), JsonOptions)
            ?? throw new InvalidOperationException("The observed culture must round trip.");

        CaptureConstructionClockContext context = actual.ConstructionObservation!.BeforeFactory.Context;
        context.CultureName.Should().BeEmpty();
        context.DateSeparator.Should().Be(separator);
        context.TimeSeparator.Should().Be(separator);
        context.AMDesignator.Should().Be(separator);
        context.PMDesignator.Should().Be(separator);
    }

    [Test]
    public void Matching_deep_endpoint_values_should_produce_only_a_stable_bracket_inference()
    {
        CaptureConstructionClockSample before = CreateSample();
        CaptureConstructionClockSample after = CreateSample(ObservedInstant.AddMilliseconds(100));
        before.Context.Should().NotBeSameAs(after.Context);
        before.Context.ConfiguredFont.Should().NotBeSameAs(after.Context.ConfiguredFont);
        before.Context.Should().Be(after.Context);

        CaptureConstructionObservation observation = CaptureConstructionClock.Create(before, after, [], []);

        observation.Boundary.Should().Be("rootFactory");
        observation.Assessment.Should().Be(CaptureConstructionBracketAssessment.StableBracketInference);
        observation.BeforeFactory.Should().BeSameAs(before);
        observation.AfterFactory.Should().BeSameAs(after);
    }

    [TestCase("text")]
    [TestCase("backwardClock")]
    [TestCase("offset")]
    [TestCase("culture")]
    [TestCase("uiCulture")]
    [TestCase("datePattern")]
    [TestCase("timePattern")]
    [TestCase("dateSeparator")]
    [TestCase("timeSeparator")]
    [TestCase("amDesignator")]
    [TestCase("pmDesignator")]
    [TestCase("calendar")]
    [TestCase("timeZone")]
    [TestCase("fontFamily")]
    [TestCase("fontSize")]
    [TestCase("bold")]
    [TestCase("italic")]
    [TestCase("underline")]
    [TestCase("strikeout")]
    [TestCase("relativeDate")]
    public void Changed_endpoints_should_remain_ambiguous(string change)
    {
        CaptureConstructionClockSample before = CreateSample();
        CaptureConstructionClockSample after = ChangeSample(CreateSample(ObservedInstant.AddMilliseconds(100)), change);

        CaptureConstructionObservation observation = CaptureConstructionClock.Create(before, after, [], []);

        observation.Assessment.Should().Be(CaptureConstructionBracketAssessment.Ambiguous);
        CreateEntry(observation).ConstructionObservation!.Assessment.Should().Be(CaptureConstructionBracketAssessment.Ambiguous);
        if (change == "backwardClock")
        {
            observation.AfterFactory.ObservedAtUtc.Should().BeBefore(observation.BeforeFactory.ObservedAtUtc);
        }
    }

    [Test]
    public void Second_rollover_should_not_claim_the_constructor_used_either_endpoint_text()
    {
        CaptureConstructionClockSample before = CreateSample(ObservedInstant.AddMilliseconds(899));
        CaptureConstructionClockSample after = CreateSample(ObservedInstant.AddMilliseconds(901));
        before.GeneralDateTimeText.Should().NotBe(after.GeneralDateTimeText);

        CaptureConstructionObservation observation = CaptureConstructionClock.Create(before, after, [], []);

        observation.Assessment.Should().Be(CaptureConstructionBracketAssessment.Ambiguous);
        observation.BeforeFactory.GeneralDateTimeText.Should().Be(before.GeneralDateTimeText);
        observation.AfterFactory.GeneralDateTimeText.Should().Be(after.GeneralDateTimeText);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Legacy_or_explicit_null_manifest_observations_should_remain_absent_when_serialized(bool explicitNull)
    {
        CaptureManifestEntry entry = CreateEntry();
        string legacy = JsonSerializer.Serialize(entry, JsonOptions);
        JsonObject json = JsonNode.Parse(legacy)!.AsObject();
        if (explicitNull)
        {
            json["constructionObservation"] = null;
        }

        CaptureManifestEntry actual = JsonSerializer.Deserialize<CaptureManifestEntry>(json.ToJsonString(), JsonOptions)
            ?? throw new InvalidOperationException("The legacy manifest entry must deserialize.");

        actual.ConstructionObservation.Should().BeNull();
        legacy.Should().NotContain("constructionObservation");
        JsonSerializer.Serialize(actual, JsonOptions).Should().Be(legacy);
    }

    [Test]
    public void Manifest_observations_should_round_trip_both_width_phases_without_changing_acquisition_or_fixture_times()
    {
        CaptureConstructionDateColumn postFactory = CreateColumn();
        CaptureConstructionDateColumn beforeImage = postFactory with
        {
            Width = 131.5,
            Unit = CaptureConstructionWidthUnit.DeviceIndependentPixel,
            Dpi = new CaptureDpi { X = 120, Y = 120 },
            RenderScale = 1.25,
        };
        CaptureConstructionObservation observation = CaptureConstructionClock.Create(
            CreateSample(), CreateSample(ObservedInstant.AddMilliseconds(100)), [postFactory], [beforeImage]);
        CaptureSetManifest manifest = new()
        {
            SchemaVersion = 1,
            CreatedAtUtc = ObservedInstant.UtcDateTime.AddMinutes(5),
            FixtureNowUtc = new DateTime(2026, 10, 23, 14, 9, 56, DateTimeKind.Utc),
            ToolVersion = "Tests",
            Repository = "fixture",
            Captures = [CreateEntry(observation)],
        };
        string json = JsonSerializer.Serialize(manifest, JsonOptions);

        CaptureSetManifest actual = JsonSerializer.Deserialize<CaptureSetManifest>(json, JsonOptions)
            ?? throw new InvalidOperationException("The construction manifest must round trip.");

        actual.CreatedAtUtc.Should().Be(manifest.CreatedAtUtc);
        actual.FixtureNowUtc.Should().Be(manifest.FixtureNowUtc);
        actual.Captures.Single().ConstructionObservation.Should().BeEquivalentTo(observation);
        JsonSerializer.Serialize(actual, JsonOptions).Should().Be(json);
        json.Should().Contain("\"boundary\": \"rootFactory\"");
        json.Should().Contain("\"assessment\": \"stableBracketInference\"");
        json.Should().Contain("\"unit\": \"physicalPixel\"");
        json.Should().Contain("\"unit\": \"deviceIndependentPixel\"");
    }

    [Test]
    public void An_unreached_image_phase_should_retain_an_empty_list_not_an_invented_width()
    {
        CaptureConstructionDateColumn column = CreateColumn();
        CaptureConstructionObservation observation = CaptureConstructionClock.Create(CreateSample(), CreateSample(), [column], []);

        CaptureManifestEntry entry = CreateEntry(observation) with { Status = CaptureStateStatus.Failed };

        entry.ConstructionObservation!.AfterFactoryDateColumns.Should().Equal([column]);
        entry.ConstructionObservation.BeforeImageDateColumns.Should().BeEmpty();
    }

    [TestCase("assessment")]
    [TestCase("changedContextClaimingStable")]
    [TestCase("boundary")]
    [TestCase("utcKind")]
    [TestCase("instantMismatch")]
    [TestCase("nullContext")]
    [TestCase("nullFont")]
    [TestCase("nullList")]
    [TestCase("nullColumn")]
    [TestCase("blankOwner")]
    [TestCase("blankProvider")]
    [TestCase("negativeIndex")]
    [TestCase("negativeWidth")]
    [TestCase("unknownUnit")]
    [TestCase("zeroDpi")]
    [TestCase("zeroScale")]
    [TestCase("blankPattern")]
    [TestCase("nullSeparator")]
    [TestCase("blankCulture")]
    public void Invalid_incoming_observation_JSON_should_be_rejected_at_the_manifest_boundary(string change)
    {
        CaptureConstructionObservation observation = CaptureConstructionClock.Create(CreateSample(), CreateSample(), [CreateColumn()], []);
        JsonObject entry = JsonNode.Parse(JsonSerializer.Serialize(CreateEntry(observation), JsonOptions))!.AsObject();
        JsonObject bracket = entry["constructionObservation"]!.AsObject();
        JsonObject before = bracket["beforeFactory"]!.AsObject();
        JsonObject context = before["context"]!.AsObject();
        JsonObject column = bracket["afterFactoryDateColumns"]!.AsArray()[0]!.AsObject();
        switch (change)
        {
            case "assessment": bracket["assessment"] = "ambiguous"; break;
            case "changedContextClaimingStable": context["relativeDate"] = true; break;
            case "boundary": bracket["boundary"] = "privateConstructorClock"; break;
            case "utcKind": before["observedAtUtc"] = "2026-11-03T12:34:56.1"; break;
            case "instantMismatch": before["observedAtUtc"] = ObservedInstant.AddTicks(1).UtcDateTime; break;
            case "nullContext": before["context"] = null; break;
            case "nullFont": context["configuredFont"] = null; break;
            case "nullList": bracket["afterFactoryDateColumns"] = null; break;
            case "nullColumn": bracket["afterFactoryDateColumns"]!.AsArray()[0] = null; break;
            case "blankOwner": column["owner"] = " "; break;
            case "blankProvider": column["provider"] = string.Empty; break;
            case "negativeIndex": column["columnIndex"] = -1; break;
            case "negativeWidth": column["width"] = -1; break;
            case "unknownUnit": column["unit"] = 999; break;
            case "zeroDpi": column["dpi"]!["x"] = 0; break;
            case "zeroScale": column["renderScale"] = 0; break;
            case "blankPattern": context["shortDatePattern"] = " "; break;
            case "nullSeparator": context["timeSeparator"] = null; break;
            case "blankCulture": context["cultureName"] = " "; break;
            default: throw new ArgumentOutOfRangeException(nameof(change));
        }

        Action deserialize = () => JsonSerializer.Deserialize<CaptureManifestEntry>(entry.ToJsonString(), JsonOptions);

        deserialize.Should().Throw<InvalidDataException>();
    }

    [TestCase("font", double.NaN)]
    [TestCase("font", double.PositiveInfinity)]
    [TestCase("font", double.NegativeInfinity)]
    [TestCase("font", 0)]
    [TestCase("font", -1)]
    [TestCase("width", double.NaN)]
    [TestCase("width", double.PositiveInfinity)]
    [TestCase("width", double.NegativeInfinity)]
    [TestCase("scale", double.NaN)]
    [TestCase("scale", double.PositiveInfinity)]
    [TestCase("scale", double.NegativeInfinity)]
    public void Nonfinite_or_invalid_measurements_should_not_enter_a_manifest(string property, double value)
    {
        CaptureConstructionClockSample sample = CreateSample();
        CaptureConstructionDateColumn column = CreateColumn();
        CaptureConstructionObservation observation = CaptureConstructionClock.Create(sample, sample, [column], []);
        observation = property switch
        {
            "font" => observation with
            {
                BeforeFactory = sample with { Context = sample.Context with { ConfiguredFont = sample.Context.ConfiguredFont with { SizePoints = value } } },
            },
            "width" => observation with { AfterFactoryDateColumns = [column with { Width = value }] },
            "scale" => observation with { AfterFactoryDateColumns = [column with { RenderScale = value }] },
            _ => throw new ArgumentOutOfRangeException(nameof(property)),
        };

        Action create = () => CreateEntry(observation);

        create.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Zero_width_and_root_owner_should_remain_valid_observations()
    {
        CaptureConstructionDateColumn column = CreateColumn() with { Owner = "$", ColumnIndex = 0, Width = 0 };

        CaptureConstructionObservation observation = CaptureConstructionClock.Create(CreateSample(), CreateSample(), [column], []);

        CreateEntry(observation).ConstructionObservation!.AfterFactoryDateColumns.Should().Equal([column]);
    }

    [Test]
    public void Live_observations_should_not_extend_the_deterministic_tree_contract()
    {
        typeof(CaptureDocument).GetProperty(nameof(CaptureManifestEntry.ConstructionObservation)).Should().BeNull();
        typeof(CaptureMetadata).GetProperty(nameof(CaptureManifestEntry.ConstructionObservation)).Should().BeNull();
        string schema = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "tree.schema.json"));
        schema.Should().NotContain("constructionObservation").And.NotContain("observedAtUtc").And.NotContain("beforeFactory");
    }

    private static CaptureConstructionClockSample ChangeSample(CaptureConstructionClockSample sample, string change)
    {
        CaptureConstructionClockContext context = sample.Context;
        CaptureConstructionFont font = context.ConfiguredFont;
        return change switch
        {
            "text" => sample with { GeneralDateTimeText = sample.GeneralDateTimeText + " changed" },
            "backwardClock" => CreateSample(ObservedInstant.AddMilliseconds(-50)),
            "offset" => sample with { ObservedAtLocal = sample.ObservedAtLocal.ToOffset(TimeSpan.FromHours(1)) },
            "culture" => sample with { Context = context with { CultureName = "en-US" } },
            "uiCulture" => sample with { Context = context with { UICultureName = "en-US" } },
            "datePattern" => sample with { Context = context with { ShortDatePattern = "yyyy-MM-dd" } },
            "timePattern" => sample with { Context = context with { LongTimePattern = "HH.mm.ss" } },
            "dateSeparator" => sample with { Context = context with { DateSeparator = "|" } },
            "timeSeparator" => sample with { Context = context with { TimeSeparator = "|" } },
            "amDesignator" => sample with { Context = context with { AMDesignator = "morning" } },
            "pmDesignator" => sample with { Context = context with { PMDesignator = "afternoon" } },
            "calendar" => sample with { Context = context with { CalendarType = "DifferentCalendar" } },
            "timeZone" => sample with { Context = context with { TimeZoneId = "Different/Zone" } },
            "fontFamily" => sample with { Context = context with { ConfiguredFont = font with { Family = "Different font" } } },
            "fontSize" => sample with { Context = context with { ConfiguredFont = font with { SizePoints = font.SizePoints + 1 } } },
            "bold" => sample with { Context = context with { ConfiguredFont = font with { Bold = true } } },
            "italic" => sample with { Context = context with { ConfiguredFont = font with { Italic = true } } },
            "underline" => sample with { Context = context with { ConfiguredFont = font with { Underline = true } } },
            "strikeout" => sample with { Context = context with { ConfiguredFont = font with { Strikeout = true } } },
            "relativeDate" => sample with { Context = context with { RelativeDate = true } },
            _ => throw new ArgumentOutOfRangeException(nameof(change)),
        };
    }

    private static CaptureConstructionClockSample CreateSample(DateTimeOffset? instant = null)
        => CaptureConstructionClock.Sample(instant ?? ObservedInstant, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture,
            TimeZoneInfo.Utc, CreateFont(), relativeDate: false);

    private static CaptureConstructionFont CreateFont()
        => new() { Family = "Fixture font", SizePoints = 9, Bold = false, Italic = false, Underline = false, Strikeout = false };

    private static CaptureConstructionDateColumn CreateColumn()
        => new()
        {
            Owner = "RevisionGrid",
            Provider = "GitUI.UserControls.RevisionGrid.Columns.DateColumnProvider",
            ColumnIndex = 5,
            Width = 132,
            Unit = CaptureConstructionWidthUnit.PhysicalPixel,
            Dpi = new CaptureDpi { X = 96, Y = 96 },
            RenderScale = 1,
        };

    private static CaptureManifestEntry CreateEntry(CaptureConstructionObservation? observation = null)
        => new()
        {
            ComponentType = "Tests.Form",
            ThemeId = "light",
            ScalePercent = 100,
            State = "normal",
            Status = CaptureStateStatus.Captured,
            CaptureMethod = CaptureMethod.PrintWindow,
            ConstructionObservation = observation,
        };
}
