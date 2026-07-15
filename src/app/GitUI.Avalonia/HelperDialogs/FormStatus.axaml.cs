using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using GitUI.Models;
using GitUI.UserControls;

namespace GitUI.HelperDialogs;

// Twin of GitUI/HelperDialogs/FormStatus.cs. Windows-only taskbar progress and the status
// badge window icons are not ported; the console is always the plain text emulator.
public partial class FormStatus : GitExtensionsDialog
{
    private readonly bool _useDialogSettings;
    private bool _errorOccurred;

    private protected Action<FormStatus>? ProcessCallback;
    private protected Action<FormStatus>? AbortCallback;

    /// <summary>For the visual designer and the XAML loader only, like WinForms.</summary>
    /// <remarks>
    ///  The dialog is built the same way as at run time, but without commands: starting an
    ///  operation from it is an error, which <see cref="GitModuleForm.UICommands"/> reports.
    /// </remarks>
    public FormStatus()
        : this(commands: null, PlainTextConsoleEmulatorsRegistry.Instance, useDialogSettings: false)
    {
    }

    /// <param name="commands">The commands, or <see langword="null"/> for the designer.</param>
    public FormStatus(IGitUICommands? commands, IConsoleEmulatorsRegistry consoleEmulatorsRegistry, bool useDialogSettings)
        : base(commands, enablePositionRestore: true)
    {
        _useDialogSettings = useDialogSettings;

        ConsoleCommandRunner = consoleEmulatorsRegistry.CreateCommandController();

        ConsoleCommandRunner.ConsoleHostTerminated += (s, e) =>
        {
            // This means the control is not visible anymore, no use in keeping.
            Close();
        };

        InitializeComponent();

        // Event hookups, like the Designer file does in WinForms (Avalonia's compiled XAML
        // requires exact RoutedEventArgs handler signatures, which the ported handlers keep
        // from upstream as EventArgs).
        ShowPassword.IsCheckedChanged += ShowPassword_CheckedChanged;
        KeepDialogOpen.IsCheckedChanged += KeepDialogOpen_CheckedChanged;
        Ok.Click += Ok_Click;
        Abort.Click += Abort_Click;
        PasswordInput.PasswordEntered += PasswordInput_PasswordEntered;

        pnlOutput.Children.Add(ConsoleCommandRunner.Control);

        ShowPassword.IsChecked = AppSettings.ShowProcessDialogPasswordInput.Value;

        if (_useDialogSettings)
        {
            KeepDialogOpen.IsChecked = !AppSettings.CloseProcessDialog;
        }
        else
        {
            KeepDialogOpen.IsVisible = false;
        }

        InitializeComplete();
    }

    public string? ProcessString { get; protected init; }
    public string? ProcessArguments { get; set; }

    private protected IConsoleCommandRunner ConsoleCommandRunner { get; }

    /// <summary>
    /// Gets the logged output text. Note that this is a separate string from what you see in the console output control.
    /// For instance, progress messages might be skipped; other messages might be added manually.
    /// </summary>
    private protected FormStatusOutputLog OutputLog { get; } = new();

    public bool ErrorOccurred()
    {
        return _errorOccurred;
    }

    public string GetOutputString()
    {
        return OutputLog.GetString();
    }

    public void Retry()
    {
        Reset();
        ProcessCallback?.Invoke(this);
    }

    public static void ShowErrorDialog(IWin32Window? owner, IGitUICommands commands, string text, params string[] output)
    {
        using FormStatus form = new(commands, consoleEmulatorsRegistry: PlainTextConsoleEmulatorsRegistry.Instance, useDialogSettings: true);
        form.Text = text;
        if (output?.Length > 0)
        {
            IPlainTextConsoleCommandRunner commandRunner = (IPlainTextConsoleCommandRunner)form.ConsoleCommandRunner;
            foreach (string line in output)
            {
                commandRunner.WriteOutputText(line);
            }
        }

        form.ProgressBar.IsVisible = false;
        form.KeepDialogOpen.IsVisible = false;
        form.ShowPassword.IsVisible = false;
        form.PasswordInput.IsVisible = false;
        form.Abort.IsVisible = false;

        // We know that an operation (whatever it may have been) has failed, so set the error state.
        form.Done(isSuccess: false);

        form.ShowDialog(owner);
    }

    protected override void OnClosed(EventArgs e)
    {
        (ConsoleCommandRunner as IDisposable)?.Dispose();
        base.OnClosed(e);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        // If the dialog was invoked via ShowErrorDialog, there is no operation to invoke
        // it has already failed (i.e. completed).
        if (!_errorOccurred)
        {
            Start();
        }
    }

    private protected void Done(bool isSuccess)
    {
        _errorOccurred = !isSuccess;

        try
        {
            RunProcessInfo runProcessInfo = new(ProcessString!, ProcessArguments!, GetOutputString(), DateTime.Now);
            UICommands.GetRequiredService<IOutputHistoryRecorder>().RecordHistory(runProcessInfo);
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }

        ShowPassword.IsVisible = false;
        PasswordInput.IsVisible = false;
        ProgressBar.IsVisible = false;
        Ok.IsEnabled = true;
        Ok.Focus();
        AcceptButton = Ok;
        Abort.IsEnabled = false;

        if (isSuccess && (_useDialogSettings && AppSettings.CloseProcessDialog))
        {
            Close();
        }
    }

    private protected void Reset()
    {
        ConsoleCommandRunner.ResetConsole();
        OutputLog.Clear();
        ShowPassword.IsVisible = true;
        PasswordInput.IsVisible = ShowPassword.IsChecked != false;
        ProgressBar.IsVisible = true;
        Ok.IsEnabled = false;
        ActiveControl = PasswordInput.IsVisible ? PasswordInput : null;
    }

    private protected async Task SetProgressAsync(string text)
    {
        // This has to happen on the UI thread
        await this.SwitchToMainThreadAsync();

        int index = text.LastIndexOf('%');
        if (index > 4 && int.TryParse(text.AsSpan(index - 3, 3), out int progressValue) && progressValue >= 0)
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = Math.Min(100, progressValue);
        }

        // Show last progress message in the title, unless it's showing in the control body already
        if (ConsoleCommandRunner is IPlainTextConsoleCommandRunner)
        {
            Text = text;
        }
    }

    private void Start()
    {
        if (ProcessCallback is null)
        {
            throw new InvalidOperationException("You can't load the form without a ProcessCallback");
        }

        if (AbortCallback is null)
        {
            Abort.IsVisible = false;
        }

        Reset();
        ProcessCallback(this);
    }

    private void Abort_Click(object sender, EventArgs e)
    {
        try
        {
            AbortCallback?.Invoke(this);
            OutputLog.Append(Environment.NewLine + "Aborted");  // TODO: write to display control also, if we pull the function up to this base class
            Done(isSuccess: false);
            DialogResult = DialogResult.Abort;
        }
        catch
        {
        }
    }

    private void KeepDialogOpen_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.CloseProcessDialog = KeepDialogOpen.IsChecked != true;

        // Maintain the invariant: if changing to "don't keep" and conditions are such that the dialog would have closed in dont-keep mode, then close it
        // Not checking for UseDialogSettings because checkbox is only visible with True
        if ((KeepDialogOpen.IsChecked != true /* keep off */) && Ok.IsEnabled /* done */ && (!_errorOccurred /* and successful */))
        {
            Close();
        }
    }

    private void ShowPassword_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.ShowProcessDialogPasswordInput.Value = ShowPassword.IsChecked == true;
        PasswordInput.IsVisible = ShowPassword.IsChecked != false;
    }

    private void PasswordInput_PasswordEntered(object sender, TextEventArgs e)
    {
        ConsoleCommandRunner.WriteCommandProcessInput($"{e.Text}\n");
    }

    private void Ok_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }
}
