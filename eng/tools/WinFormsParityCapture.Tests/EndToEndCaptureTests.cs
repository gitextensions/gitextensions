using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class EndToEndCaptureTests
{
    [Test]
    public async Task CaptureAsync_should_reject_repository_inside_working_tree_Async()
    {
        CaptureOptions options = new()
        {
            Command = CaptureCommand.Capture,
            PlanPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "capture-plan.json"),
            RepositoryPath = Environment.CurrentDirectory,
            OutputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
        };

        Func<Task> action = async () => await CaptureRunner.CaptureAsync(options);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*outside this working tree*");
    }

    [Test]
    public void Parse_should_require_capture_values()
    {
        CaptureOptions options = CaptureOptions.Parse(
        [
            "capture",
            "--plan", "plan.json",
            "--repository", "repo",
            "--output", "output",
            "--scales", "100,200"
        ]);

        options.Command.Should().Be(CaptureCommand.Capture);
        options.Scales.Should().BeEquivalentTo([100, 200]);
    }

    [Test]
    [Apartment(ApartmentState.MTA)]
    public void Bootstrap_should_reject_a_non_sta_thread()
    {
        CaptureSettingsProfile profile = new()
        {
            UiFontFamily = "Segoe UI",
            UiFontSizePoints = 9,
            FixedFontFamily = "Consolas",
            FixedFontSizePoints = 10,
            AppSettings = new Dictionary<string, string>()
        };
        CaptureThemePlan theme = new()
        {
            Id = "light",
            Kind = "builtin",
            File = "invariant.css",
            IsBuiltin = true
        };

        Action action = () => WinFormsBootstrap.Create(Environment.CurrentDirectory, profile, theme, AppContext.BaseDirectory);

        action.Should().Throw<ThreadStateException>()
            .WithMessage("*STA*");
    }
}
