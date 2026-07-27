using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MessageColumnProvider : ColumnProvider
{
    private record struct Settings(
        bool FillRefLabels,
        bool ShowRemoteBranches,
        bool ShowGitStatusForArtificialCommits,
        bool ShowRevisionGridTooltips,
        bool ShowTags);

    public const int MaxSuperprojectRefs = 4;

    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    private readonly RevisionGridControl _grid;
    private IReadOnlyDictionary<string, AheadBehindData>? _aheadBehindDataByLocalBranch;
    private IReadOnlyDictionary<string, AheadBehindData>? _aheadBehindDataByRemoteBranch;
    private IAheadBehindDataProvider? _aheadBehindDataProvider;
    private Settings _settings;

    public MessageColumnProvider(RevisionGridControl grid)
        : base("Message", new GridLength(1, GridUnitType.Star), minimumWidth: 25, resizable: true)
    {
        _grid = grid;
    }

    public void SetAheadBehindDataProvider(IAheadBehindDataProvider? provider)
    {
        _aheadBehindDataProvider = provider;
        Clear();
    }

    public override void ApplySettings()
    {
        Column.IsVisible = true;
        _settings = new Settings(
            AppSettings.FillRefLabels,
            AppSettings.ShowRemoteBranches,
            AppSettings.ShowGitStatusForArtificialCommits,
            AppSettings.ShowRevisionGridTooltips.Value,
            AppSettings.ShowTags);
    }

    public override void Clear()
    {
        _aheadBehindDataByLocalBranch = null;
        _aheadBehindDataByRemoteBranch = null;
    }

    public override Control CreateCell()
    {
        MessageCell panel = new(this)
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(ColumnLeftMargin, 0, 2, 0),
            ClipToBounds = true,
        };
        panel.Classes.Add("revision-message-cell");
        panel.Subject.Classes.Add("revision-subject");
        panel.Children.Add(panel.Subject);
        return panel;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        MessageCell panel = (MessageCell)control;
        panel.Children.RemoveRange(0, panel.Children.Count - 1);

        if (revision.IsArtificial)
        {
            RevisionGridRefRenderer.RefLabelControl artificialLabel =
                RevisionGridRefRenderer.CreateSpecialLabel(
                    revision.Subject,
                    revision.ObjectId == ObjectId.IndexId
                        ? RefLabelIcon.CommitIndex
                        : RefLabelIcon.WorkingDirectory,
                    dashed: false);
            artificialLabel.MinWidth = GetArtificialLabelWidth(panel);
            panel.Children.Insert(panel.Children.Count - 1, artificialLabel);

            foreach (Control statusControl in CreateArtificialStatusControls(revision.ObjectId))
            {
                panel.Children.Insert(panel.Children.Count - 1, statusControl);
            }

            panel.Subject.Text = string.Empty;
            panel.Subject.FontWeight = FontWeight.Normal;
            panel.Revision = revision;
            panel.ClearHighlight();
            ToolTip.SetTip(panel, GetArtificialToolTip(revision));
            return;
        }

        SuperProjectInfo? superProjectInfo = _grid.TryGetSuperProjectInfo(out SuperProjectInfo? info)
            ? info
            : null;
        IReadOnlyList<IGitRef>? superprojectRefs = superProjectInfo?.Refs is not null
            && superProjectInfo.Refs.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs)
                ? refs
                : null;
        foreach (Control label in CreateSuperprojectLabels(revision, superProjectInfo))
        {
            panel.Children.Insert(panel.Children.Count - 1, label);
        }

        foreach (Control label in RevisionGridRefRenderer.CreateLabels(
                     revision.Refs,
                     _settings.ShowTags,
                     _settings.ShowRemoteBranches,
                     _settings.FillRefLabels,
                     GetVirtualRef,
                     superprojectRefs?.Select(gitRef => gitRef.CompleteName).ToHashSet(StringComparer.Ordinal)))
        {
            panel.Children.Insert(panel.Children.Count - 1, label);
        }

        panel.Subject.Text = revision.Subject;
        panel.Subject.FontWeight = _grid.IsCurrentCheckout(revision)
            ? FontWeight.Bold
            : FontWeight.Normal;
        panel.Revision = revision;
        panel.ClearHighlight();
        ToolTip.SetTip(panel, _settings.ShowRevisionGridTooltips ? revision.Subject : null);
    }

    private static double GetArtificialLabelWidth(MessageCell panel)
    {
        double fontSize = panel.Subject.FontSize > 0 ? panel.Subject.FontSize : 12;
        Typeface typeface = new(
            panel.Subject.FontFamily,
            panel.Subject.FontStyle,
            panel.Subject.FontWeight);
        double workTreeWidth = Measure(ResourceManager.TranslatedStrings.Workspace);
        double indexWidth = Measure(ResourceManager.TranslatedStrings.Index);

        // RefLabelControl adds eight pixels of horizontal padding, one-pixel overlap
        // correction, and a five-pixel right margin.
        return Math.Ceiling(Math.Max(workTreeWidth, indexWidth)) + 12;

        double Measure(string text)
            => new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black).Width;
    }

    private IReadOnlyList<Control> CreateArtificialStatusControls(ObjectId objectId)
    {
        if (!_settings.ShowGitStatusForArtificialCommits
            || _grid.GetChangeCount(objectId) is not ArtificialCommitChangeCount changeCount)
        {
            return [];
        }

        if (!changeCount.DataValid)
        {
            return [CreateArtificialCount(icon: Images.RepoStateUnknown, count: null)];
        }

        if (!changeCount.HasChanges)
        {
            return [CreateArtificialCount(icon: Images.RepoStateClean, count: null)];
        }

        List<Control> controls = [];
        Add(changeCount.Changed, Images.FileStatusModified);
        Add(changeCount.New, Images.FileStatusAdded);
        Add(changeCount.Deleted, Images.FileStatusRemoved);
        Add(changeCount.SubmodulesChanged, Images.SubmoduleRevisionDown);
        Add(changeCount.SubmodulesDirty, Images.SubmoduleDirty);
        return controls;

        void Add(IReadOnlyList<GitItemStatus> items, IImage icon)
        {
            if (items.Count > 0)
            {
                controls.Add(CreateArtificialCount(icon, items.Count));
            }
        }
    }

    private string? GetArtificialToolTip(GitRevision revision)
    {
        if (_settings.ShowGitStatusForArtificialCommits
            && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount)
        {
            return changeCount.GetSummary();
        }

        return _settings.ShowRevisionGridTooltips ? revision.Subject : null;
    }

    private static Control CreateArtificialCount(IImage icon, int? count)
    {
        StackPanel panel = new()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 3,
            Margin = new Thickness(1, 0, 5, 0),
        };
        panel.Children.Add(new Image
        {
            Source = icon,
            Width = 12,
            Height = 12,
            VerticalAlignment = VerticalAlignment.Center,
        });
        if (count is int value)
        {
            panel.Children.Add(new TextBlock
            {
                Text = value.ToString(),
                MinWidth = 14,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return panel;
    }

    internal static IReadOnlyList<Control> CreateSuperprojectLabels(
        GitRevision revision,
        SuperProjectInfo? superProjectInfo)
    {
        if (superProjectInfo is null)
        {
            return [];
        }

        List<Control> labels = [];
        if (superProjectInfo.CurrentCommit == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel(string.Empty, RefLabelIcon.Head));
        }

        if (superProjectInfo.ConflictBase == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Base", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.ConflictLocal == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Local", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.ConflictRemote == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Remote", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.Refs?.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs) == true)
        {
            IEnumerable<IGitRef> additionalRefs = refs
                .Where(superProjectRef => revision.Refs.All(gitRef => gitRef.CompleteName != superProjectRef.CompleteName))
                .Take(MaxSuperprojectRefs);
            foreach (IGitRef gitRef in additionalRefs)
            {
                labels.Add(RevisionGridRefRenderer.CreateLabel(
                    gitRef,
                    gitRef.Name,
                    gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect,
                    fill: false,
                    dashed: true));
            }
        }

        return labels;
    }

    private IGitRef? GetVirtualRef(IGitRef gitRef)
    {
        (string display, string trackedCompleteName) = GetAheadBehind(gitRef, withCounts: false);
        if (display.Length == 0)
        {
            return null;
        }

        return new VirtualRef(
            display,
            trackedCompleteName,
            gitRef.TrackingRemote,
            gitRef.CompleteName,
            gitRef.Module)
        {
            IsHead = gitRef.IsRemote,
            IsRemote = !gitRef.IsRemote,
        };
    }

    private (string Display, string TrackedCompleteName) GetAheadBehind(IGitRef gitRef, bool withCounts = true)
    {
        _aheadBehindDataByLocalBranch ??= _aheadBehindDataProvider?.GetData()
            ?? FrozenDictionary<string, AheadBehindData>.Empty;

        if (gitRef.IsRemote)
        {
            _aheadBehindDataByRemoteBranch ??= _aheadBehindDataByLocalBranch.Values
                .DistinctBy(data => data.RemoteRef)
                .ToFrozenDictionary(data => data.RemoteRef, data => data);

            if (_aheadBehindDataByRemoteBranch.TryGetValue(gitRef.CompleteName, out AheadBehindData aheadBehind))
            {
                return (aheadBehind.ToDisplay(withCounts), GitRefName.RefsHeadsPrefix + aheadBehind.Branch);
            }
        }
        else if (_aheadBehindDataByLocalBranch.TryGetValue(gitRef.Name, out AheadBehindData aheadBehind))
        {
            return (aheadBehind.ToDisplay(withCounts, reverse: true), aheadBehind.RemoteRef);
        }

        return (string.Empty, string.Empty);
    }

    private AheadBehindData? GetAheadBehindData(bool isRemote, string completeName)
    {
        _aheadBehindDataByLocalBranch ??= _aheadBehindDataProvider?.GetData()
            ?? FrozenDictionary<string, AheadBehindData>.Empty;

        if (isRemote)
        {
            _aheadBehindDataByRemoteBranch ??= _aheadBehindDataByLocalBranch.Values
                .DistinctBy(data => data.RemoteRef)
                .ToFrozenDictionary(data => data.RemoteRef, data => data);
            return _aheadBehindDataByRemoteBranch.TryGetValue(completeName, out AheadBehindData dataByRemote)
                ? dataByRemote
                : null;
        }

        string branchName = completeName.StartsWith(GitRefName.RefsHeadsPrefix, StringComparison.Ordinal)
            ? completeName[GitRefName.RefsHeadsPrefix.Length..]
            : completeName;
        return _aheadBehindDataByLocalBranch.TryGetValue(branchName, out AheadBehindData data)
            ? data
            : null;
    }

    private string? GetRefToolTip(IGitRef? gitRef)
    {
        if (gitRef is null)
        {
            return null;
        }

        StringBuilder toolTip = new();
        if (gitRef.Guid is null)
        {
            bool realRefIsRemote = !gitRef.IsRemote;
            string realRefCompleteName = gitRef.MergeWith;
            string realRefName = RemovePrefix(
                realRefCompleteName,
                realRefIsRemote ? GitRefName.RefsRemotesPrefix : GitRefName.RefsHeadsPrefix);
            AheadBehindData? data = GetAheadBehindData(realRefIsRemote, realRefCompleteName);
            toolTip.Append('[').Append(realRefName).Append(']');
            if (realRefIsRemote)
            {
                toolTip.AppendLine().AppendFormat(
                    TranslatedStrings.IsTrackedBy_Branch_AheadBehind,
                    data?.Branch,
                    data?.ToDisplay());
            }
            else
            {
                AppendTrackingDetails(toolTip, data);
            }

            return toolTip.ToString();
        }

        AheadBehindData? aheadBehind = GetAheadBehindData(gitRef.IsRemote, gitRef.CompleteName);
        toolTip.Append('[').Append(gitRef.Name).Append(']');
        if (gitRef.IsRemote)
        {
            if (aheadBehind is not null)
            {
                toolTip.AppendLine().AppendFormat(
                    TranslatedStrings.IsTrackedBy_Branch_AheadBehind,
                    aheadBehind.Value.Branch,
                    aheadBehind.Value.ToDisplay());
            }
            else if (_settings.ShowRevisionGridTooltips)
            {
                toolTip.AppendLine().Append(TranslatedStrings.IsRemoteBranch);
            }
            else
            {
                return null;
            }
        }
        else if (gitRef.IsHead)
        {
            if (aheadBehind is not null)
            {
                AppendTrackingDetails(toolTip, aheadBehind);
            }
            else if (_settings.ShowRevisionGridTooltips)
            {
                toolTip.AppendLine().Append(TranslatedStrings.IsLocalBranch);
            }
            else
            {
                return null;
            }
        }
        else if (gitRef.IsTag)
        {
            if (_settings.ShowRevisionGridTooltips)
            {
                toolTip.AppendLine().Append(TranslatedStrings.IsTag);
            }
            else
            {
                return null;
            }
        }

        return toolTip.ToString();

        static void AppendTrackingDetails(StringBuilder builder, AheadBehindData? data)
        {
            string? remoteBranch = data is null
                ? null
                : RemovePrefix(data.Value.RemoteRef, GitRefName.RefsRemotesPrefix);
            if (data?.AheadCount == AheadBehindData.Gone)
            {
                builder.AppendLine().AppendFormat(TranslatedStrings.WasTracking_Remote, remoteBranch);
            }
            else
            {
                builder.Append("   ").AppendLine(data?.ToDisplay())
                    .AppendFormat(TranslatedStrings.IsTracking_Remote, remoteBranch);
            }
        }

        static string RemovePrefix(string value, string prefix)
            => value.StartsWith(prefix, StringComparison.Ordinal) ? value[prefix.Length..] : value;
    }

    private sealed class MessageCell : StackPanel
    {
        private readonly MessageColumnProvider _provider;
        private RevisionGridRefRenderer.RefLabelControl? _highlightedLabel;

        public MessageCell(MessageColumnProvider provider)
        {
            _provider = provider;
            PointerMoved += OnPointerMoved;
            PointerExited += (_, _) =>
            {
                ClearHighlight();
                ToolTip.SetTip(this, _provider._settings.ShowRevisionGridTooltips ? Revision?.Subject : null);
            };
            DoubleTapped += OnDoubleTapped;
        }

        public TextBlock Subject { get; } = CreateTextBlock();

        public GitRevision? Revision { get; set; }

        public void ClearHighlight()
        {
            if (_highlightedLabel is not null)
            {
                _highlightedLabel.IsHighlighted = false;
                _highlightedLabel = null;
            }

            Cursor = null;
        }

        private RevisionGridRefRenderer.RefLabelControl? HitTest(Func<Visual, Avalonia.Point> getPosition)
            => this.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .FirstOrDefault(label => label.Contains(getPosition(label)));

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            RevisionGridRefRenderer.RefLabelControl? label = HitTest(e.GetPosition);
            if (!ReferenceEquals(label, _highlightedLabel))
            {
                ClearHighlight();
                _highlightedLabel = label;
                if (label is not null)
                {
                    label.IsHighlighted = true;
                    Cursor = HandCursor;
                }
            }

            ToolTip.SetTip(this, label is null
                ? _provider._settings.ShowRevisionGridTooltips ? Revision?.Subject : null
                : _provider.GetRefToolTip(label.GitRef));
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (HitTest(e.GetPosition) is { GitRef: not null } label
                && _provider._grid.GoToRelatedRef(label.GitRef))
            {
                e.Handled = true;
            }
        }
    }

    private sealed class VirtualRef(
        string name,
        string completeName,
        string remote,
        string mergeWith,
        IGitModule module) : IGitRef
    {
        public string Name => name;
        public ObjectId ObjectId => throw new NotSupportedException();
        public string? Guid => null;
        public IGitModule Module => module;
        public string CompleteName => completeName;
        public string Remote => remote;
        public string LocalName => Name;
        public bool IsRemote { get; init; }
        public bool IsHead { get; init; }
        public bool IsTag => false;
        public bool IsBisect => false;
        public bool IsBisectGood => false;
        public bool IsBisectBad => false;
        public bool IsStash => false;
        public bool IsDereference => false;
        public bool IsSelected { get; set; }
        public bool IsSelectedHeadMergeSource { get; set; }
        public string MergeWith
        {
            get => mergeWith;
            set => throw new NotSupportedException();
        }

        public string TrackingRemote
        {
            get => string.Empty;
            set => throw new NotSupportedException();
        }

        public bool IsTrackingRemote(IGitRef? remoteRef) => false;

        public override bool Equals(object? obj)
            => obj is VirtualRef other && CompleteName == other.CompleteName;

        public override int GetHashCode() => completeName.GetHashCode();
    }
}
