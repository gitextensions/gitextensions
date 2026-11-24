using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using ResourceManager;
using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

namespace GitExtensions.Plugins.GitImpact;

public sealed partial class FormImpact : GitExtensionsFormBase
{
    private readonly TranslationString _authorCommits = new("{0} ({1} Commits, {2} Changed Lines)");

    public FormImpact()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
        UpdateAuthorInfo(string.Empty);
    }

    public FormImpact(IGitModule module)
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
        UpdateAuthorInfo(string.Empty);
        Impact.Init(module);
        Impact.UpdateData();
        Impact.Invalidated += Impact_Invalidated;
    }

    private void WireControls()
    {
        Impact.PointerMoved += Impact_PointerMoved;
        cbIncludingSubmodules.IsCheckedChanged += cbShowSubmodules_CheckedChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        Impact.Stop();

        base.OnClosed(e);

        Impact.Dispose();
    }

    private void Impact_Invalidated(object? sender, EventArgs e)
    {
        UpdateAuthorInfo(Impact.SelectedAuthor);
    }

    private void UpdateAuthorInfo(string author)
    {
        bool hasAuthor = !string.IsNullOrEmpty(author);
        lblAuthor.IsVisible = hasAuthor;
        pnlAuthorColor.IsVisible = hasAuthor;

        if (hasAuthor)
        {
            ImpactLoader.DataPoint data = Impact.GetAuthorInfo(author);
            lblAuthor.Text = string.Format(_authorCommits.Text, author, data.Commits, data.ChangedLines);
            pnlAuthorColor.Background = new SolidColorBrush(Impact.GetAuthorColor(author));
        }
    }

    private void Impact_PointerMoved(object? sender, PointerEventArgs e)
    {
        Avalonia.Point point = e.GetPosition(Impact);
        if (Impact.TrySetAuthorByScreenPosition((int)point.X, (int)point.Y))
        {
            Impact.Invalidate();
        }
    }

    private void cbShowSubmodules_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateAuthorInfo(string.Empty);
        Impact.ShowSubmodules = cbIncludingSubmodules.IsChecked == true;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormImpact form)
    {
        public ImpactControl Impact => form.Impact;
        public bool IsAuthorVisible => form.lblAuthor.IsVisible && form.pnlAuthorColor.IsVisible;
        public AvaloniaCheckBox IncludingSubmodules => form.cbIncludingSubmodules;
    }
}
