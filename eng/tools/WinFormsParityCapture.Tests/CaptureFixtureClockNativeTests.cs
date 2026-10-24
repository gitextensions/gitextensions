using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using NUnit.Framework;
using ResourceManager.CommitDataRenders;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Exercises the capture-only clock through the unchanged native header and owned Browse lifetime.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class CaptureFixtureClockNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const string EnumerationFailure = "The bounded capture-owner enumeration failed.";
    private const string RefreshAuthor = "Capture clock refresh author";
    private const string RestoredAuthor = "Restored live clock author";
    private static readonly DateTime FixtureNowUtc = new(2026, 7, 17, 12, 0, 0, DateTimeKind.Utc);

    [TestCase(false)]
    [TestCase(true)]
    public void Actual_source_header_should_use_only_an_explicit_capture_clock_and_restore_its_original_formatters(bool supplyClock)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("CaptureFixtureClockProbe");
        probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
        {
            Control[] controls = [.. DescendantControls(form)];
            HeaderValues[] originals = [.. controls.OfType<CommitInfoHeader>().Select(Snapshot)];
            originals.Should().NotBeEmpty("the actual Browse must own its original commit-header instances before Show");
            CommitInfo info = ReadField<CommitInfo>(form, "RevisionInfo");
            CommitInfoHeader header = ReadField<CommitInfoHeader>(info, "commitInfoHeader");
            RichTextBox text = ReadField<RichTextBox>(header, "rtbRevisionHeader");
            HeaderValues mainOriginal = originals.Single(value => ReferenceEquals(value.Header, header));
            List<object> observations = [];
            using ApplicationContext loop = new();
            using System.Windows.Forms.Timer timer = new() { Interval = 50 };
            Stopwatch elapsed = Stopwatch.StartNew();
            CaptureFixtureClock? clock = null;
            GitRevision? refreshRevision = null;
            Exception? failure = null;
            int stage = 0;
            bool finishing = false;
            EventHandler onIdle = null!;
            onIdle = (_, _) =>
            {
                Application.Idle -= onIdle;
                try
                {
                    using (CaptureFixtureClock absent = CaptureFixtureClock.Apply(YieldThenFail([]), null))
                    {
                        AssertRestored(originals);
                    }

                    Action failedAcquisition = () => CaptureFixtureClock.Apply(YieldThenFail(controls), FixtureNowUtc);
                    failedAcquisition.Should().Throw<InvalidOperationException>().WithMessage(EnumerationFailure);
                    AssertRestored(originals);

                    clock = CaptureFixtureClock.Apply(controls, supplyClock ? FixtureNowUtc : null);
                    foreach (HeaderValues original in originals)
                    {
                        HeaderValues applied = Snapshot(original.Header);
                        applied.HeaderFormatter.Should().BeSameAs(applied.RendererFormatter);
                        if (supplyClock)
                        {
                            applied.HeaderFormatter.Should().NotBeSameAs(original.HeaderFormatter);
                        }
                        else
                        {
                            applied.HeaderFormatter.Should().BeSameAs(original.HeaderFormatter);
                        }
                    }

                    form.Show();
                    NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                    timer.Start();
                }
                catch (Exception exception)
                {
                    failure = exception;
                    Finish();
                }
            };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                try
                {
                    if (getThreadException() is Exception threadException)
                    {
                        ExceptionDispatchInfo.Capture(threadException).Throw();
                    }

                    if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                    {
                        throw new TimeoutException($"The actual source clock probe did not finish stage {stage}.");
                    }

                    if (!IsLoadingComplete(form.RevisionGridControl) || info.Revision is not GitRevision revision
                        || revision.ObjectId.IsArtificial || string.IsNullOrWhiteSpace(text.Text))
                    {
                        return;
                    }

                    IDateFormatter active = ReadField<IDateFormatter>(header, "_dateFormatter");
                    switch (stage)
                    {
                        case 0:
                            text.Text.Should().Contain(active.FormatDateAsRelativeLocal(revision.AuthorDate), "the first real repository-backed header must use the formatter installed before Show");
                            Record("first-render", revision, active);
                            refreshRevision = CreateRevision(revision, RefreshAuthor);
                            header.ShowCommitInfo(refreshRevision, []);
                            break;
                        case 1:
                            if (!text.Text.Contains(RefreshAuthor, StringComparison.Ordinal))
                            {
                                return;
                            }

                            GitRevision refreshed = refreshRevision
                                ?? throw new InvalidOperationException("The real header refresh must retain its source revision input.");
                            string expected = supplyClock
                                ? new DateFormatter(() => FixtureNowUtc).FormatDateAsRelativeLocal(refreshed.AuthorDate)
                                : mainOriginal.HeaderFormatter.FormatDateAsRelativeLocal(refreshed.AuthorDate);
                            text.Text.Should().Contain(expected, "a later ShowCommitInfo must use the same explicit clock or its untouched live formatter");
                            Record("later-refresh", refreshed, active);
                            clock?.Dispose();
                            AssertRestored(originals);
                            refreshRevision = CreateRevision(revision, RestoredAuthor);
                            header.ShowCommitInfo(refreshRevision, []);
                            break;
                        default:
                            if (!text.Text.Contains(RestoredAuthor, StringComparison.Ordinal))
                            {
                                return;
                            }

                            GitRevision restored = refreshRevision
                                ?? throw new InvalidOperationException("The restored real header must retain its source revision input.");
                            AssertRestored(originals);
                            text.Text.Should().Contain(mainOriginal.HeaderFormatter.FormatDateAsRelativeLocal(restored.AuthorDate), "disposing the capture scope must restore actual live rendering, not only a field marker");
                            Record("restored-live-refresh", restored, mainOriginal.HeaderFormatter);
                            Finish();
                            break;
                    }

                    stage++;
                }
                catch (Exception exception)
                {
                    failure = exception;
                    if (form.Visible)
                    {
                        try
                        {
                            Record("failed", refreshRevision ?? info.Revision, ReadField<IDateFormatter>(header, "_dateFormatter"));
                        }
                        catch (Exception diagnosticFailure)
                        {
                            TestContext.Out.WriteLine($"captureClockSecondaryDiagnosticFailure={diagnosticFailure}");
                        }
                    }

                    Finish();
                }
                finally
                {
                    if (!finishing)
                    {
                        timer.Start();
                    }
                }
            };
            Application.Idle += onIdle;
            try
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(30, 30);
                form.ClientSize = new Size(1520, 950);
                Application.Run(loop);
            }
            finally
            {
                timer.Stop();
                Application.Idle -= onIdle;
                clock?.Dispose();
                AssertRestored(originals);
                File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "clock-observations.json"), JsonSerializer.Serialize(new
                {
                    supplyClock,
                    fixtureNowUtc = supplyClock ? FixtureNowUtc : (DateTime?)null,
                    completedStages = stage,
                    failure = failure?.ToString(),
                    observations,
                    acquisition = "actual original Browse-owned header, capture-only constructor-injected DateFormatter input before first Show; real ShowCommitInfo refresh and exact original-identity restoration; no global clock, captured-text replacement or repository writes",
                }, new JsonSerializerOptions { WriteIndented = true }));
            }

            if (failure is not null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }

            stage.Should().Be(3);
            return;

            void Finish()
            {
                if (finishing)
                {
                    return;
                }

                finishing = true;
                timer.Stop();
                clock?.Dispose();
                closeAndDrain();
                loop.ExitThread();
            }

            void Record(string name, GitRevision? revision, IDateFormatter formatter)
            {
                observations.Add(new
                {
                    name,
                    stage,
                    actualUtcNow = DateTime.UtcNow,
                    revision = revision?.ObjectId.ToString(),
                    revision?.AuthorUnixTime,
                    expectedDate = revision is null ? null : formatter.FormatDateAsRelativeLocal(revision.AuthorDate),
                    actualHeaderText = text.Text,
                    headerAndRendererShareFormatter = ReferenceEquals(Snapshot(header).HeaderFormatter, Snapshot(header).RendererFormatter),
                    originalFormatterRestored = ReferenceEquals(Snapshot(header).HeaderFormatter, mainOriginal.HeaderFormatter),
                });
                using CaptureImageResult frame = ImageCapture.Capture(form, [], []);
                frame.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, name + ".png"), ImageFormat.Png);
            }
        });
    }

    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Unspecified)]
    public void Apply_should_reject_a_non_UTC_clock_before_enumerating_owners(DateTimeKind kind)
    {
        DateTime invalid = DateTime.SpecifyKind(FixtureNowUtc, kind);
        Action apply = () => CaptureFixtureClock.Apply(YieldThenFail([]), invalid);
        apply.Should().Throw<InvalidDataException>().WithMessage("*UTC*");
    }

    private static void AssertRestored(IEnumerable<HeaderValues> originals)
    {
        foreach (HeaderValues original in originals)
        {
            HeaderValues current = Snapshot(original.Header);
            current.HeaderFormatter.Should().BeSameAs(original.HeaderFormatter);
            current.Renderer.Should().BeSameAs(original.Renderer);
            current.RendererFormatter.Should().BeSameAs(original.RendererFormatter);
        }
    }

    private static GitRevision CreateRevision(GitRevision original, string author)
    {
        GitRevision revision = original.Clone();
        revision.Author = author;
        revision.AuthorEmail = "fixture-clock@example.test";
        revision.Committer = author;
        revision.CommitterEmail = revision.AuthorEmail;
        revision.AuthorUnixTime = new DateTimeOffset(FixtureNowUtc.AddMinutes(-90)).ToUnixTimeSeconds();
        revision.CommitUnixTime = revision.AuthorUnixTime;
        return revision;
    }

    private static IEnumerable<Control> DescendantControls(Control root)
    {
        yield return root;
        foreach (Control child in root.Controls)
        {
            foreach (Control descendant in DescendantControls(child))
            {
                yield return descendant;
            }
        }
    }

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = typeof(RevisionGridControl).GetMethod("GetTestAccessor", InstanceMembers)?.Invoke(grid, null)
            ?? throw new MissingMethodException(typeof(RevisionGridControl).FullName, "GetTestAccessor");
        return (bool)(accessor.GetType().GetProperty("IsDataLoadComplete", InstanceMembers)?.GetValue(accessor)
            ?? throw new MissingMemberException(accessor.GetType().FullName, "IsDataLoadComplete"));
    }

    private static T ReadField<T>(object owner, string name) where T : class
        => owner.GetType().GetField(name, InstanceMembers)?.GetValue(owner) as T
           ?? throw new MissingFieldException(owner.GetType().FullName, name);

    private static HeaderValues Snapshot(CommitInfoHeader header)
    {
        CommitDataHeaderRenderer renderer = ReadField<CommitDataHeaderRenderer>(header, "_commitDataHeaderRenderer");
        return new HeaderValues(header, ReadField<IDateFormatter>(header, "_dateFormatter"), renderer, ReadField<IDateFormatter>(renderer, "_dateFormatter"));
    }

    private static IEnumerable<object> YieldThenFail(IEnumerable<object> controls)
    {
        foreach (object control in controls)
        {
            yield return control;
        }

        throw new InvalidOperationException(EnumerationFailure);
    }

    private sealed record HeaderValues(CommitInfoHeader Header, IDateFormatter HeaderFormatter, CommitDataHeaderRenderer Renderer, IDateFormatter RendererFormatter);
}
