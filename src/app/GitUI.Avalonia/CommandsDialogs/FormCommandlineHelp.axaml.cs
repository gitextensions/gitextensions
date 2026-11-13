namespace GitUI.CommandsDialogs;

public partial class FormCommandlineHelp : GitExtensionsForm
{
    // The command list is a resx-backed _NO_TRANSLATE_ label in the original; it is seeded here.
    private const string CommandsText = """
        [path]
        browse [path] [-filter=] [--pathFilter=<filepath>] [-commit=<selectedSha>[,<firstSha>]]
        about
        add [filename]
        addfiles [filename]
        apply [filename]
        applypatch [filename]
        blame filename
        branch
        checkout
        checkoutbranch
        checkoutrevision
        cherry
        cleanup
        clone [path]
        commit [--quiet] [--message commitmessage]
        difftool filename
        filehistory filename
        fileeditor filename
        formatpatch
        gitignore
        help (shows this dialog)
        init [path]
        merge [--branch name]
        mergeconflicts [--quiet]
        mergetool [--quiet]
        openrepo [path] [-filter=]
        pull [--rebase] [--merge] [--fetch] [--quiet] [--remotebranch name]
        push [--quiet]
        rebase [--branch name]
        remotes
        reset
        revert filename
        searchfile
        settings
        stash
        synchronize [--rebase] [--merge] [--fetch] [--quiet]
        tag
        viewdiff
        viewpatch [filename]
        """;

    public FormCommandlineHelp()
    {
        InitializeComponent();
        _NO_TRANSLATE_commands.Text = CommandsText;
        InitializeComplete();
    }
}
