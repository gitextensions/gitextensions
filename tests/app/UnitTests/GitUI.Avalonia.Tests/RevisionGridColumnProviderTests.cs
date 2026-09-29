using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.Avatars;
using GitUI.Compat;
using GitUI.Properties;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RevisionGridColumnProviderTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [Test]
    public void Revision_grid_should_register_the_WinForms_column_provider_order()
    {
        RevisionGridControl control = new();

        control.ColumnProviders.Select(provider => provider.GetType()).Should().Equal(
            typeof(RevisionGraphColumnProvider),
            typeof(MessageColumnProvider),
            typeof(NotesColumnProvider),
            typeof(AvatarColumnProvider),
            typeof(AuthorNameColumnProvider),
            typeof(DateColumnProvider),
            typeof(CommitIdColumnProvider),
            typeof(BuildStatusColumnProvider));
        control.ColumnProviders.Select(provider => provider.Name).Should().Equal(
            "Graph",
            "Message",
            "Notes",
            "Avatar",
            "Author Name",
            "Date",
            "Commit ID",
            "Build Status");
        control.ColumnProviders.Select(provider => provider.Index).Should().Equal(Enumerable.Range(0, 8));
        control.ColumnProviders[3].Column.Width.Should().Be(new GridLength(32));
        control.ColumnProviders[6].Column.Resizable.Should().BeFalse();
        control.ColumnProviders[7].Column.Width.Should().Be(new GridLength(150));
        control.ColumnProviders[7].Column.Resizable.Should().BeTrue();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.3")]
    public void Capture_reader_should_emit_the_native_revision_grid_column_model()
    {
        RevisionGridControl control = new();
        Window window = new() { Width = 900, Height = 160, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            CaptureNode grid = Flatten(
                    new AvaloniaControlTreeReader(control, renderScale: 1.25)
                        .ReadPrimary(control, new Avalonia.PixelSize(900, 160)).Root)
                .Single(node => node.FieldName == "_gridView");

            grid.Columns.Select(column => column.HeaderText).Should().Equal(
                string.Empty,
                "Message",
                "Notes",
                "Avatar",
                "Author Name",
                "Date",
                "Commit ID",
                "Build Status");
            grid.Columns.Select(column => column.Index).Should().Equal(Enumerable.Range(0, 8));
            grid.Columns.Select(column => column.FieldName).Should().Equal(
                null,
                "_maximizedColumn",
                null,
                null,
                null,
                null,
                "_lastVisibleResizableColumn",
                null);
            grid.Columns.Should().OnlyContain(column => column.SortMode == "NotSortable");
            grid.Columns.Should().OnlyContain(column => column.Alignment == "NotSet");
            grid.Columns.Should().OnlyContain(column => column.HeaderAlignment == "NotSet");
            grid.Columns.Select(column => column.Colors.InactiveSelectionBackground).Should().NotContainNulls();
            grid.Columns.Select(column => column.Colors.DisabledForeground).Should().NotContainNulls();
            grid.ControlKind.Should().Be("dataGrid");
            grid.ReadOnly.Should().BeTrue();
            grid.Children.Should().BeEmpty("recycled Avalonia rows are renderer details rather than semantic child controls");
            grid.Colors.SelectionBackground.Should().NotBeNull();
            grid.Colors.InactiveSelectionBackground.Should().NotBeNull();
            grid.Colors.DisabledForeground.Should().NotBeNull();
            grid.Colors.GridLine.Should().NotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Revision_grid_should_apply_column_visibility_and_width_settings()
    {
        ColumnSettings original = ColumnSettings.Capture();
        AvatarProvider originalAvatarProvider = AppSettings.AvatarProvider;
        AvatarFallbackType originalAvatarFallback = AppSettings.AvatarFallbackType;
        try
        {
            AppSettings.ShowRevisionGridGraphColumn = true;
            AppSettings.ShowGitNotesColumn.Value = true;
            AppSettings.ShowAuthorAvatarColumn = true;
            AppSettings.ShowAuthorNameColumn = false;
            AppSettings.ShowDateColumn = true;
            AppSettings.ShowObjectIdColumn = true;
            AppSettings.AvatarProvider = AvatarProvider.None;
            AppSettings.AvatarFallbackType = AvatarFallbackType.AuthorInitials;
            AvatarService.UpdateAvatarProvider();

            RevisionGridControl control = new();
            ListBox revisions = control.FindControl<ListBox>("_gridView")
                ?? throw new InvalidOperationException("The revision list was not created.");
            revisions.ItemsSource = new[] { CreateRevision() };
            Window window = new()
            {
                Width = 900,
                Height = 160,
                Content = control,
            };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();

                Grid row = control.GetVisualDescendants()
                    .OfType<Grid>()
                    .Single(grid => grid.Classes.Contains("revision-row"));
                row.ColumnDefinitions.Select(column => column.Width).Should().Equal(
                    new GridLength(22),
                    new GridLength(1, GridUnitType.Star),
                    new GridLength(50),
                    new GridLength(26),
                    new GridLength(0),
                    new GridLength(130),
                    new GridLength(60),
                    new GridLength(0));
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-avatar-cell"))
                    .IsVisible.Should().BeTrue();
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-author-cell"))
                    .IsVisible.Should().BeFalse();

                AppSettings.ShowGitNotesColumn.Value = false;
                AppSettings.ShowAuthorNameColumn = true;
                control.ApplyColumnSettings();

                row.ColumnDefinitions[2].Width.Should().Be(new GridLength(0));
                row.ColumnDefinitions[4].Width.Should().Be(new GridLength(130));
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-notes-cell"))
                    .IsVisible.Should().BeFalse();
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-author-cell"))
                    .IsVisible.Should().BeTrue();
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            original.Restore();
            AppSettings.AvatarProvider = originalAvatarProvider;
            AppSettings.AvatarFallbackType = originalAvatarFallback;
            AvatarService.UpdateAvatarProvider();
        }
    }

    [AvaloniaTest]
    public void Revision_grid_column_providers_should_format_dates_notes_ids_and_tooltips()
    {
        GitRevision revision = CreateRevision();
        DateTime now = revision.CommitDate.AddHours(3);

        DateColumnProvider.GetDate(revision, showAuthorDate: true).Should().Be(revision.AuthorDate);
        DateColumnProvider.GetDate(revision, showAuthorDate: false).Should().Be(revision.CommitDate);
        DateColumnProvider.FormatDate(revision.CommitDate, now, relative: true).Should().Be(
            LocalizationHelpers.GetRelativeDateString(now, revision.CommitDate, displayWeeks: false));
        DateColumnProvider.FormatDate(revision.CommitDate, now, relative: false).Should().Be(revision.CommitDate.ToString("G"));
        NotesColumnProvider.FirstLine(revision.Notes).Should().Be("First note");
        RevisionGridControl grid = new();
        AuthorNameColumnProvider authorProvider = new(grid, new AuthorRevisionHighlighting());
        authorProvider.TryGetToolTip(revision, out string? authorToolTip).Should().BeTrue();
        authorToolTip.Should().Contain("Author <author@example.com>");
        authorToolTip.Should().Contain("Committer <committer@example.com>");

        CommitIdColumnProvider idProvider = new(grid);
        idProvider.TryGetToolTip(revision, out string? idToolTip).Should().BeTrue();
        idToolTip.Should().Be(revision.Guid);
        TextBlock idCell = (TextBlock)idProvider.CreateCell();
        idProvider.Column.Width = new GridLength(55);
        idProvider.OnColumnWidthChanged();
        idProvider.UpdateCell(idCell, revision);
        idCell.Text.Should().NotBeNullOrEmpty();
        idCell.Text!.Length.Should().BeLessThan(ObjectId.Sha1CharCount);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Revision_grid_columns_should_preserve_source_text_and_deferred_detail_behavior()
    {
        bool originalRelativeDate = AppSettings.RelativeDate;
        WinFormsShims.Font originalFont = AppSettings.Font;
        ICommitDataManager commitDataManager = Substitute.For<ICommitDataManager>();
        try
        {
            AppSettings.RelativeDate = false;
            AppSettings.Font = new WinFormsShims.Font("Segoe UI", 9);
            GitRevision revision = CreateRevision();
            revision.Notes = null;

            RevisionGridControl grid = new();
            NotesColumnProvider notesProvider = new(grid, commitDataManager);
            Control notesCell = notesProvider.CreateCell();
            notesProvider.UpdateCell(notesCell, revision);

            ((TextBlock)notesCell).Text.Should().BeEmpty();
            commitDataManager.Received(1).InitiateDelayedLoadingOfDetails(revision);

            AuthorNameColumnProvider authorProvider = new(grid, new AuthorRevisionHighlighting());
            DateTime widthWindowStart = DateTime.Now;
            DateColumnProvider dateProvider = new(grid);
            DateTime widthWindowEnd = DateTime.Now;
            BuildStatusColumnProvider buildProvider = new(_ => { }, () => Substitute.For<IGitModule>());
            authorProvider.CreateCell().Opacity.Should().Be(1,
                "the source author column uses the row's unmodified foreground");
            dateProvider.CreateCell().Opacity.Should().Be(1,
                "the source date column uses the row's unmodified foreground");
            TextBlock widthProbe = new()
            {
                FontFamily = new FontFamily(AppSettings.Font.Name),
                FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
            };
            List<double> sourceMeasuredWidths = [];
            for (DateTime sample = widthWindowStart.AddSeconds(-1);
                 sample <= widthWindowEnd.AddSeconds(1);
                 sample = sample.AddSeconds(1))
            {
                sourceMeasuredWidths.Add(WinFormsTextMeasurer.MeasureTextRenderer(widthProbe, sample.ToString("G")).Width);
            }

            sourceMeasuredWidths.Should().Contain(dateProvider.Column.Width.Value,
                "the source measures the current absolute-date text instead of using a fixed column width");
            buildProvider.CreateCell().Classes.Should().Contain("gitextensions-commit-header",
                "the source paints build status with its configured monospace font");
        }
        finally
        {
            AppSettings.RelativeDate = originalRelativeDate;
            AppSettings.Font = originalFont;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Multiline_indicator_should_only_reserve_space_when_the_source_cell_can_fit_it()
    {
        RevisionGridControl control = new();
        MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
            .Single(column => column.Name == "Message");
        GitRevision revision = CreateRevision();
        revision.HasMultiLineMessage = true;
        Control cell = provider.CreateCell();
        provider.UpdateCell(cell, revision);
        AutomationProperties.GetName(cell).Should().Be(revision.Subject.Trim());
        Window window = new() { Width = 40, Height = 40, Content = cell };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            MultilineIndicator indicator = cell.GetVisualDescendants().OfType<MultilineIndicator>().Single();
            indicator.IsVisible.Should().BeFalse(
                "WinForms suppresses the 26-DIP indicator unless twice that width is available");

            window.Width = 100;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            indicator.IsVisible.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Message_provider_should_apply_ref_filters_fill_and_virtual_ahead_behind_labels()
    {
        bool originalFill = AppSettings.FillRefLabels;
        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        bool originalShowTags = AppSettings.ShowTags;
        bool originalDrawNonRelativesGray = AppSettings.RevisionGraphDrawNonRelativesGray;
        try
        {
            AppSettings.FillRefLabels = true;
            AppSettings.ShowRemoteBranches = false;
            AppSettings.ShowTags = false;
            AppSettings.RevisionGraphDrawNonRelativesGray = false;

            IGitModule module = Substitute.For<IGitModule>();
            GitRevision revision = CreateRevision();
            revision.Refs =
            [
                CreateRef(module, revision.ObjectId, "main", "refs/heads/main", isHead: true),
                CreateRef(module, revision.ObjectId, "origin/main", "refs/remotes/origin/main", isRemote: true),
                CreateRef(module, revision.ObjectId, "v1", "refs/tags/v1", isTag: true),
            ];
            IAheadBehindDataProvider aheadBehindProvider = Substitute.For<IAheadBehindDataProvider>();
            aheadBehindProvider.GetData(Arg.Any<string>()).Returns(
                new Dictionary<string, AheadBehindData>
                {
                    ["main"] = new("main", "refs/remotes/origin/main", AheadCount: "1", BehindCount: string.Empty),
                });

            RevisionGridControl control = new();
            control.SetAheadBehindDataProvider(aheadBehindProvider);
            control.ApplyColumnSettings();
            ListBox revisions = control.FindControl<ListBox>("_gridView")
                ?? throw new InvalidOperationException("The revision list was not created.");
            revisions.ItemsSource = new[] { revision };
            Window window = new()
            {
                Width = 900,
                Height = 160,
                Content = control,
            };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();

                RevisionGridRefRenderer.RefLabelControl[] labels =
                [
                    .. control.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>(),
                ];
                labels.Select(label => label.Label).Should().Equal("main", "↓");
                labels.Should().OnlyContain(label => label.Fill);
                RevisionGridRefRenderer.RefLabelControl virtualLabel = labels.Single(label => label.Label == "↓");
                virtualLabel.IsDashed.Should().BeTrue();
                NestledVirtualRef nestledRef = virtualLabel.GitRef.Should().BeOfType<NestledVirtualRef>().Subject;
                nestledRef.CompleteName.Should().Be("refs/remotes/origin/main");
                nestledRef.IsRemote.Should().BeTrue();
                nestledRef.TrackingBranchIsGone.Should().BeFalse();
                ((RevisionGraphColumnProvider)control.ColumnProviders[0]).RevisionGraphDrawStyle
                    .Should().Be(RevisionGraphDrawStyle.Normal);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.FillRefLabels = originalFill;
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
            AppSettings.ShowTags = originalShowTags;
            AppSettings.RevisionGraphDrawNonRelativesGray = originalDrawNonRelativesGray;
        }
    }

    [AvaloniaTest]
    public void Message_provider_should_render_superproject_checkout_and_additional_refs()
    {
        GitRevision revision = CreateRevision();
        IGitModule module = Substitute.For<IGitModule>();
        IGitRef superprojectRef = CreateRef(
            module,
            revision.ObjectId,
            "release",
            "refs/heads/release");
        IGitRef existingRef = CreateRef(
            module,
            revision.ObjectId,
            "main",
            "refs/heads/main",
            isHead: true);
        revision.Refs = [existingRef];
        SuperProjectInfo superProjectInfo = new()
        {
            CurrentCommit = revision.ObjectId,
            Refs = new Dictionary<ObjectId, IReadOnlyList<IGitRef>>
            {
                [revision.ObjectId] = [existingRef, superprojectRef],
            },
        };

        RevisionGridRefRenderer.RefLabelControl[] labels =
        [
            .. MessageColumnProvider.CreateSuperprojectLabels(revision, superProjectInfo)
                .Cast<RevisionGridRefRenderer.RefLabelControl>(),
        ];

        labels.Should().HaveCount(2);
        labels[0].Icon.Should().Be(RefLabelIcon.Head);
        labels[0].Label.Should().BeEmpty();
        labels[1].Label.Should().Be("release");
        labels[1].IsDashed.Should().BeTrue();

        RevisionGridRefRenderer.RefLabelControl existingLabel = RevisionGridRefRenderer.CreateLabels(
                revision.Refs,
                showTags: true,
                showRemoteBranches: true,
                fill: false,
                getVirtualRef: null,
                superprojectRefs: new HashSet<string>(StringComparer.Ordinal) { existingRef.CompleteName })
            .Should().ContainSingle()
            .Which.Should().BeOfType<RevisionGridRefRenderer.RefLabelControl>().Subject;
        existingLabel.IsDashed.Should().BeTrue();
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Revision_graph_provider_should_preserve_the_source_cache_lifecycle_for_retained_rows()
    {
        RevisionGridControl control = new();
        RevisionGraphColumnProvider provider = (RevisionGraphColumnProvider)control.ColumnProviders[0];
        RevisionGraphColumnProvider.TestAccessor accessor = provider.GetTestAccessor();
        VisibleRowRange range = new(fromIndex: 2, count: 4);

        accessor.RenderGraphToCache(range, toRowIndex: 5, rowHeight: 22);

        accessor.CachedVisibleRange.Equals(range).Should().BeTrue();
        accessor.LastRenderedRow.Should().Be(5);
        provider.Clear();
        accessor.LastRenderedRow.Should().Be(-1);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Avatar_provider_should_show_the_source_placeholder_while_loading()
    {
        TaskCompletionSource<byte[]?> avatarCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider avatarProvider = Substitute.For<IAvatarProvider>();
        avatarProvider.GetAvatarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int>())
            .Returns(avatarCompletion.Task);
        RevisionGridControl grid = new();
        AvatarColumnProvider provider = new(grid, avatarProvider, Substitute.For<IAvatarCacheCleaner>());
        Image cell = (Image)provider.CreateCell();

        provider.OnCellPainting(cell, CreateRevision());

        cell.Source.Should().BeSameAs(Images.User80);
        avatarCompletion.SetResult(null);
    }

    [AvaloniaTest]
    public void Message_provider_should_render_bisect_markers_like_the_original()
    {
        RevisionGridControl control = new();
        MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
            .Single(column => column.Name == "Message");
        GitRevision revision = CreateRevision();
        IGitRef good = CreateRef(Substitute.For<IGitModule>(), revision.ObjectId, "good", "refs/bisect/good-1");
        IGitRef bad = CreateRef(Substitute.For<IGitModule>(), revision.ObjectId, "bad", "refs/bisect/bad");
        good.IsBisectGood.Returns(true);
        bad.IsBisectBad.Returns(true);
        revision.Refs = [good, bad];
        Control cell = provider.CreateCell();

        provider.UpdateCell(cell, revision);

        cell.GetVisualDescendants().OfType<Image>()
            .Count(image => image.Classes.Contains("revision-bisect-marker"))
            .Should().Be(2);
        cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()
            .Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Message_provider_should_compact_matching_remote_branch_until_hovered()
    {
        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        try
        {
            AppSettings.ShowRemoteBranches = true;
            RevisionGridControl control = new();
            MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
                .Single(column => column.Name == "Message");
            provider.ApplySettings();
            GitRevision revision = CreateRevision();
            IGitModule module = Substitute.For<IGitModule>();
            module.GetEffectiveSetting("remote.upstream.prefix", string.Empty).Returns("prefix/");
            IGitRef local = CreateRef(module, revision.ObjectId, "main", "refs/heads/main", isHead: true);
            IGitRef tracked = CreateRef(module, revision.ObjectId, "origin/main", "refs/remotes/origin/main", isRemote: true);
            tracked.Remote.Returns("origin");
            tracked.LocalName.Returns("main");
            local.IsTrackingRemote(tracked).Returns(true);
            IGitRef matchingRemote = CreateRef(module, revision.ObjectId, "upstream/prefix/main", "refs/remotes/upstream/prefix/main", isRemote: true);
            matchingRemote.Remote.Returns("upstream");
            matchingRemote.LocalName.Returns("prefix/main");
            revision.Refs = [local, tracked, matchingRemote];
            Control cell = provider.CreateCell();

            provider.UpdateCell(cell, revision);

            RevisionGridRefRenderer.RefLabelControl label = cell.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .Single(item => ReferenceEquals(item.GitRef, matchingRemote));
            label.Label.Should().Be("upstream");

            label.IsHighlighted = true;

            label.Label.Should().Be("upstream/prefix/main");
        }
        finally
        {
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Message_provider_should_keep_exactly_one_ref_highlight_and_restore_it_after_refresh()
    {
        RevisionGridControl control = new();
        MessageColumnProvider provider = (MessageColumnProvider)control.ColumnProviders
            .Single(column => column.Name == "Message");
        provider.ApplySettings();
        IGitModule module = Substitute.For<IGitModule>();
        GitRevision firstRevision = CreateRevision();
        IGitRef firstRef = CreateRef(module, firstRevision.ObjectId, "main", "refs/heads/main", isHead: true);
        firstRevision.Refs = [firstRef];
        GitRevision secondRevision = new(ObjectId.Parse("abcdef1234567890abcdef1234567890abcdef12"))
        {
            Subject = "Second revision",
        };
        IGitRef secondRef = CreateRef(module, secondRevision.ObjectId, "feature", "refs/heads/feature", isHead: true);
        secondRevision.Refs = [secondRef];
        Control firstCell = provider.CreateCell();
        Control secondCell = provider.CreateCell();
        provider.UpdateCell(firstCell, firstRevision);
        provider.UpdateCell(secondCell, secondRevision);
        RevisionGridRefRenderer.RefLabelControl firstLabel = GetLabel(firstCell, firstRef);
        RevisionGridRefRenderer.RefLabelControl secondLabel = GetLabel(secondCell, secondRef);

        provider.SetHighlight(firstCell, firstLabel).Should().BeTrue();
        provider.SetHighlight(firstCell, firstLabel).Should().BeFalse();
        firstLabel.IsHighlighted.Should().BeTrue();
        firstCell.Cursor.Should().NotBeNull();

        provider.SetHighlight(secondCell, secondLabel).Should().BeTrue();
        firstLabel.IsHighlighted.Should().BeFalse();
        firstCell.Cursor.Should().BeNull();
        secondLabel.IsHighlighted.Should().BeTrue();

        provider.UpdateCell(secondCell, secondRevision);
        RevisionGridRefRenderer.RefLabelControl refreshedLabel = GetLabel(secondCell, secondRef);
        refreshedLabel.Should().NotBeSameAs(secondLabel);
        secondLabel.IsHighlighted.Should().BeFalse();
        refreshedLabel.IsHighlighted.Should().BeTrue();
        secondCell.Cursor.Should().NotBeNull();

        provider.UpdateCell(secondCell, firstRevision);
        refreshedLabel.IsHighlighted.Should().BeFalse();
        secondCell.Cursor.Should().BeNull();

        provider.SetHighlight(firstCell, firstLabel).Should().BeTrue();
        provider.Clear();
        firstLabel.IsHighlighted.Should().BeFalse();
        firstCell.Cursor.Should().BeNull();

        static RevisionGridRefRenderer.RefLabelControl GetLabel(Control cell, IGitRef gitRef)
            => cell.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .Single(label => ReferenceEquals(label.GitRef, gitRef));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Revision_grid_should_hide_its_active_tooltip_when_the_owner_window_deactivates()
    {
        bool originalTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        Window owner = new() { Width = 900, Height = 180 };
        try
        {
            AppSettings.ShowRevisionGridTooltips.Value = true;
            RevisionGridControl control = new();
            GitRevision revision = CreateRevision();
            revision.HasMultiLineMessage = true;
            revision.Body = revision.Subject + "\n\nTooltip body";
            control.GetTestAccessor().SetRevisions([revision]);
            owner.Content = control;
            owner.Show();
            owner.Activate();
            Dispatcher.UIThread.RunJobs();
            control.GetTestAccessor().OwnerWindow.Should().BeSameAs(owner);
            Control messageCell = control.GetVisualDescendants()
                .OfType<Control>()
                .Single(item => item.Classes.Contains("revision-message-cell"));
            Point point = messageCell.TranslatePoint(
                    new Point(messageCell.Bounds.Width / 2, messageCell.Bounds.Height / 2),
                    owner)
                ?? throw new InvalidOperationException("The revision message cell is not attached.");

            owner.MouseMove(point, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            ToolTip.GetTip(messageCell).Should().NotBeNull();
            ToolTip.SetIsOpen(messageCell, true);
            ToolTip.GetIsOpen(messageCell).Should().BeTrue();

            control.GetTestAccessor().RaiseOwnerWindowDeactivated();
            Dispatcher.UIThread.RunJobs();

            ToolTip.GetIsOpen(messageCell).Should().BeFalse();
        }
        finally
        {
            owner.Close();
            AppSettings.ShowRevisionGridTooltips.Value = originalTooltips;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Column_provider_should_preserve_the_source_format_paint_and_tooltip_order()
    {
        bool originalTooltips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowRevisionGridTooltips.Value = true;
            RecordingColumnProvider provider = new();
            Control cell = provider.CreateCell();

            provider.UpdateCell(cell, CreateRevision());

            provider.Stages.Should().Equal("format", "paint", "tooltip");
            ToolTip.GetTip(cell).Should().Be("provider tooltip");
        }
        finally
        {
            AppSettings.ShowRevisionGridTooltips.Value = originalTooltips;
        }
    }

    private static GitRevision CreateRevision()
        => new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Subject = "Provider-shaped revision row",
            Author = "Author",
            AuthorEmail = "author@example.com",
            AuthorUnixTime = 1_700_000_000,
            Committer = "Committer",
            CommitterEmail = "committer@example.com",
            CommitUnixTime = 1_700_003_600,
            Notes = "First note\nSecond note",
        };

    private static IEnumerable<CaptureNode> Flatten(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static IGitRef CreateRef(
        IGitModule module,
        ObjectId objectId,
        string name,
        string completeName,
        bool isHead = false,
        bool isRemote = false,
        bool isTag = false)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Module.Returns(module);
        gitRef.ObjectId.Returns(objectId);
        gitRef.Guid.Returns(objectId.ToString());
        gitRef.Name.Returns(name);
        gitRef.LocalName.Returns(name.Split('/')[^1]);
        gitRef.CompleteName.Returns(completeName);
        gitRef.IsHead.Returns(isHead);
        gitRef.IsRemote.Returns(isRemote);
        gitRef.IsTag.Returns(isTag);
        return gitRef;
    }

    private sealed class RecordingColumnProvider : ColumnProvider
    {
        public RecordingColumnProvider()
            : base("Recording", new GridLength(10), minimumWidth: 1, resizable: false)
        {
        }

        public List<string> Stages { get; } = [];

        public override Control CreateCell() => new TextBlock();

        public override void OnCellFormatting(Control control, GitRevision revision)
            => Stages.Add("format");

        public override void OnCellPainting(Control control, GitRevision revision)
            => Stages.Add("paint");

        public override bool TryGetToolTip(
            GitRevision revision,
            [NotNullWhen(returnValue: true)] out string? toolTip)
        {
            Stages.Add("tooltip");
            toolTip = "provider tooltip";
            return true;
        }
    }

    private readonly record struct ColumnSettings(
        bool ShowGraph,
        bool ShowNotes,
        bool ShowAvatar,
        bool ShowAuthor,
        bool ShowDate,
        bool ShowObjectId)
    {
        public static ColumnSettings Capture()
            => new(
                AppSettings.ShowRevisionGridGraphColumn,
                AppSettings.ShowGitNotesColumn.Value,
                AppSettings.ShowAuthorAvatarColumn,
                AppSettings.ShowAuthorNameColumn,
                AppSettings.ShowDateColumn,
                AppSettings.ShowObjectIdColumn);

        public void Restore()
        {
            AppSettings.ShowRevisionGridGraphColumn = ShowGraph;
            AppSettings.ShowGitNotesColumn.Value = ShowNotes;
            AppSettings.ShowAuthorAvatarColumn = ShowAvatar;
            AppSettings.ShowAuthorNameColumn = ShowAuthor;
            AppSettings.ShowDateColumn = ShowDate;
            AppSettings.ShowObjectIdColumn = ShowObjectId;
        }
    }
}
