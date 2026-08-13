using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using GitUIPluginInterfaces;
using DrawingColor = System.Drawing.Color;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class BuildStatusColumnProvider : ColumnProvider
{
    private const int IconColumnWidth = 16;
    private const int TextColumnWidth = 150;
    private readonly Action<GitRevision> _openBuildReport;

    public BuildStatusColumnProvider(Action<GitRevision> openBuildReport)
        : base("Build Status", new GridLength(150), minimumWidth: 16, resizable: true)
    {
        _openBuildReport = openBuildReport;
        Column.IsAvailable = false;
    }

    public override void ApplySettings()
    {
        bool showIcon = AppSettings.ShowBuildStatusIconColumn;
        bool showText = AppSettings.ShowBuildStatusTextColumn;
        Column.IsVisible = showIcon || showText;
        if (!Column.IsAvailable || !Column.IsVisible)
        {
            return;
        }

        Column.Resizable = showText;
        if (showIcon && !showText)
        {
            Column.Width = new GridLength(IconColumnWidth);
        }
        else if (showText && Math.Abs(Column.Width.Value - IconColumnWidth) < 0.001d)
        {
            Column.Width = new GridLength(TextColumnWidth);
        }
    }

    public override Control CreateCell()
    {
        BuildStatusTextBlock textBlock = new()
        {
            Margin = new Thickness(ColumnLeftMargin, 0, 2, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        textBlock.Classes.Add("revision-build-status-cell");
        textBlock.Tapped += (_, _) =>
        {
            if (textBlock.DataContext is GitRevision { BuildStatus.Url: not null } revision)
            {
                _openBuildReport(revision);
            }
        };
        return textBlock;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        BuildStatusTextBlock textBlock = (BuildStatusTextBlock)control;
        BuildInfo? buildStatus = revision.BuildStatus;
        textBlock.Text = buildStatus is null
            ? string.Empty
            : (AppSettings.ShowBuildStatusIconColumn ? buildStatus.StatusSymbol : string.Empty)
                + (AppSettings.ShowBuildStatusTextColumn ? buildStatus.Description : string.Empty);
        textBlock.Cursor = string.IsNullOrWhiteSpace(buildStatus?.Url) ? Cursor.Default : new Cursor(StandardCursorType.Hand);
        textBlock.Status = buildStatus?.Status;
        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        toolTip = revision.BuildStatus?.Tooltip ?? revision.BuildStatus?.Description;
        return toolTip is not null;
    }

    internal sealed class BuildStatusTextBlock : TextBlock
    {
        private static readonly DrawingColor LightBlue = DrawingColor.FromArgb(130, 180, 240);
        private ListBoxItem? _row;
        private BuildStatus? _status;
        private bool? _selectedForTest;

        public BuildStatusTextBlock()
        {
            ActualThemeVariantChanged += (_, _) => UpdateForeground();
        }

        public BuildStatus? Status
        {
            get => _status;
            set
            {
                _status = value;
                UpdateForeground();
            }
        }

        internal bool? SelectedForTest
        {
            get => _selectedForTest;
            set
            {
                _selectedForTest = value;
                UpdateForeground();
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _row = this.FindAncestorOfType<ListBoxItem>();
            if (_row is not null)
            {
                _row.PropertyChanged += RowPropertyChanged;
            }

            UpdateForeground();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            if (_row is not null)
            {
                _row.PropertyChanged -= RowPropertyChanged;
                _row = null;
            }

            base.OnDetachedFromVisualTree(e);
        }

        private void RowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ListBoxItem.IsSelectedProperty)
            {
                UpdateForeground();
            }
        }

        private void UpdateForeground()
        {
            bool isSelected = _selectedForTest ?? _row?.IsSelected == true;
            DrawingColor? customColor = _status switch
            {
                BuildStatus.Success => isSelected ? DrawingColor.LightGreen : DrawingColor.DarkGreen,
                BuildStatus.Failure => isSelected ? DrawingColor.Red : DrawingColor.DarkRed,
                BuildStatus.InProgress => isSelected ? LightBlue : DrawingColor.Blue,
                BuildStatus.Unstable => DrawingColor.OrangeRed,
                BuildStatus.Stopped => isSelected ? DrawingColor.LightGray : DrawingColor.Gray,
                _ => null,
            };
            if (customColor is null)
            {
                Foreground = null;
                return;
            }

            DrawingColor background = AvaloniaThemeResources.ResolveAppColor(
                ThemeModule.Settings,
                AppColor.PanelBackground);
            DrawingColor resolved = customColor.Value.AdaptForeColor(background);
            MediaColor color = AvaloniaThemeResources.ToMediaColor(resolved);
            Foreground = new SolidColorBrush(color);
        }
    }
}
