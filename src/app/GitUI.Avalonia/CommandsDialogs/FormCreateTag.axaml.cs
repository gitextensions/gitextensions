using Avalonia;
using Avalonia.Controls;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Git.Tag;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormCreateTag : GitModuleForm
{
    private readonly TranslationString _messageCaption = new("Tag");
    private readonly TranslationString _noRevisionSelected = new("Select 1 revision to create the tag on.");
    private readonly TranslationString _pushToCaption = new("Push tag to '{0}'");
    private static readonly TranslationString _trsLightweight = new("Lightweight tag");
    private static readonly TranslationString _trsAnnotated = new("Annotated tag");
    private static readonly TranslationString _trsSignDefault = new("Sign with default GPG");
    private static readonly TranslationString _trsSignSpecificKey = new("Sign with specific GPG");

    private readonly IGitTagController? _gitTagController;
    private string _currentRemote = "";

    public FormCreateTag()
    {
        InitializeComponent();
        WireEvents();
        PopulateAnnotationOptions();
        InitializeComplete();
    }

    public FormCreateTag(IGitUICommands commands, ObjectId objectId)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireEvents();
        PopulateAnnotationOptions();

        commitPickerSmallControl1.UICommandsSource = this;
        AcceptButton = Ok;

        InitializeComplete();

        if (objectId.IsArtificial)
        {
            objectId = default;
        }

        if (objectId.IsZero)
        {
            objectId = Module.GetCurrentCheckout();
        }

        if (!objectId.IsZero)
        {
            commitPickerSmallControl1.SetSelectedCommitHash(objectId.ToString());
        }

        _gitTagController = new GitTagController(commands);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        textBoxTagName.Focus();
        _currentRemote = Module.GetCurrentRemote();
        if (string.IsNullOrEmpty(_currentRemote))
        {
            _currentRemote = "origin";
        }

        pushTag.Content = string.Format(_pushToCaption.Text, _currentRemote);
    }

    private void OkClick(object? sender, EventArgs e)
    {
        try
        {
            string tagName = CreateTag();
            if (string.IsNullOrEmpty(tagName))
            {
                return;
            }

            if (pushTag.IsChecked == true)
            {
                PushTag(tagName);
            }

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string CreateTag()
    {
        ObjectId objectId = commitPickerSmallControl1.SelectedObjectId;
        if (objectId.IsZero)
        {
            MessageBoxes.Show(this, _noRevisionSelected.Text, _messageCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }

        GitCreateTagArgs createTagArgs = new(
            textBoxTagName.Text ?? string.Empty,
            objectId,
            GetSelectedOperation(annotate.SelectedIndex),
            tagMessage.Text ?? string.Empty,
            textBoxGpgKey.Text ?? string.Empty,
            ForceTag.IsChecked == true);
        IGitTagController gitTagController = _gitTagController
            ?? throw new InvalidOperationException($"{nameof(FormCreateTag)} was constructed incorrectly.");
        return gitTagController.CreateTag(createTagArgs, this)
            ? textBoxTagName.Text ?? string.Empty
            : "";
    }

    private void PushTag(string tagName)
    {
        // The existing process dialog performs the real remote push until the specialized
        // remote-process window and push event-script host are available.
        ArgumentString pushCommand = Commands.PushTag(_currentRemote, tagName, false);
        UICommands.StartGitCommandProcessDialog(this, pushCommand);
    }

    private void AnnotateDropDownChanged(object? sender, EventArgs e)
    {
        if (annotate.SelectedIndex < 0)
        {
            return;
        }

        TagOperation tagOperation = GetSelectedOperation(annotate.SelectedIndex);
        bool usesSpecificKey = tagOperation == TagOperation.SignWithSpecificKey;
        textBoxGpgKey.IsEnabled = usesSpecificKey;
        keyIdLbl.IsEnabled = usesSpecificKey;

        bool providesMessage = tagOperation.CanProvideMessage();
        tagMessage.IsEnabled = providesMessage;
        tagMessage.BorderThickness = providesMessage ? new Thickness(1) : new Thickness(0);
    }

    private static TagOperation GetSelectedOperation(int dropdownSelection)
    {
        return dropdownSelection switch
        {
            0 => TagOperation.Lightweight,
            1 => TagOperation.Annotate,
            2 => TagOperation.SignWithDefaultKey,
            3 => TagOperation.SignWithSpecificKey,
            _ => throw new NotSupportedException("Invalid dropdownSelection"),
        };
    }

    private void PopulateAnnotationOptions()
    {
        int selectedIndex = Math.Max(annotate.SelectedIndex, 0);
        annotate.ItemsSource = new[]
        {
            _trsLightweight.Text,
            _trsAnnotated.Text,
            _trsSignDefault.Text,
            _trsSignSpecificKey.Text,
        };
        annotate.SelectedIndex = selectedIndex;
    }

    private void WireEvents()
    {
        Ok.Click += OkClick;
        annotate.SelectionChanged += AnnotateDropDownChanged;
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);
        PopulateAnnotationOptions();
    }
}
